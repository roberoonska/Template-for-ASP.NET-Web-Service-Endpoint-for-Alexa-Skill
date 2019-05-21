// Code adapted from: https://shulerent.com/2018/03/18/validating-alexa-skill-web-requests-in-c/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Alexa.NET.Request;
using System.IO;


namespace TideAPI
{
    public class SecurityHelper
    {
        static readonly string SkillID = "YOUR SKILL'S SKILL ID";
        
        static readonly string CertHeader = "-----BEGIN CERTIFICATE-----";
        static readonly string CertFooter = "-----END CERTIFICATE-----";

        static HttpClient _client = new HttpClient();

        static Dictionary<string, X509Certificate2> _validatedCertificateChains = new Dictionary<string, X509Certificate2>();
        
        /// <summary>
        /// Returns 0 if there is no security issue with Request.
        /// Returns 1 if the request cannot be validated.
        /// Returns 2 if the request ID does not match.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="skillRequest"></param>
        public static async Task<int> SecurityHandler(HttpRequestMessage httpRequest, SkillRequest skillRequest)
        {
            int securityCode = 0;

            //verify the request signature
            try
            {
                bool isValidated = await ValidateRequestSecurity(httpRequest, skillRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!isValidated)
                securityCode = 1;

            //verify that the request came from our skill
            else if (skillRequest.Context.System.Application.ApplicationId != SkillID)
                securityCode = 2;

            return securityCode;
        }


        public static IEnumerable<string> ParseSubjectAlternativeNames(X509Certificate2 cert)
        {
            Regex sanRex = new Regex(@"^DNS Name=(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var sanList = from X509Extension ext in cert.Extensions
                          where ext.Oid.FriendlyName.Equals("Subject Alternative Name", StringComparison.Ordinal)
                          let data = new AsnEncodedData(ext.Oid, ext.RawData)
                          let text = data.Format(true)
                          from line in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          let match = sanRex.Match(line)
                          where match.Success && match.Groups.Count > 0 && !string.IsNullOrEmpty(match.Groups[1].Value)
                          select match.Groups[1].Value;

            return sanList;
        }
        
        public static bool ValidateCertificateChain(X509Certificate2 certificate, IEnumerable<X509Certificate2> chain)
        {
            using (var verifier = new X509Chain())
            {
                verifier.ChainPolicy.ExtraStore.AddRange(chain.ToArray());
                var result = verifier.Build(certificate);
                return result;
            }
        }

        /// <summary>
        /// Return a X500Certificate2 given a properly formatted string.
        /// </summary>
        /// <param name="base64CertificateText"></param>
        public static X509Certificate2 ParseCertificate(string base64CertificateText)
        {
            var bytes = Convert.FromBase64String(base64CertificateText);
            X509Certificate2 cert = new X509Certificate2(bytes);
            return cert;
        }

        public static async Task<X509Certificate2[]> DownloadPemCertificatesAsync(string pemUri)
        {
            var pemText = await _client.GetStringAsync(pemUri);
            if (string.IsNullOrEmpty(pemText)) return null;
            return ReadPemCertificates(pemText);
        }


        public static X509Certificate2[] ReadPemCertificates(string pemString)
        {
            var lines = pemString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> certList = new List<string>();
            StringBuilder grouper = null;
            for (int i = 0; i < lines.Length; i++)
            {
                var curLine = lines[i];
                if (curLine.Equals(CertHeader, StringComparison.Ordinal))
                {
                    grouper = new StringBuilder();
                }
                else if (curLine.Equals(CertFooter, StringComparison.Ordinal))
                {
                    certList.Add(grouper.ToString());
                    grouper = null;
                }
                else
                {
                    if (grouper != null)
                    {
                        grouper.Append(curLine);
                    }
                }
            }

            List<X509Certificate2> collection = new List<X509Certificate2>();

            foreach (var certText in certList)
            {
                var cert = ParseCertificate(certText);
                collection.Add(cert);
            }

            return collection.ToArray();
        }
        
        public static async Task ValidateRequestSecurity(HttpRequestMessage Request, SkillRequest skillRequest)
        {

            if (skillRequest == null || skillRequest.Request == null || skillRequest.Request.Timestamp == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp Missing");
            }

            var ts = skillRequest.Request.Timestamp;
            var tsDiff = (DateTimeOffset.UtcNow - ts).TotalSeconds;

            if (System.Math.Abs(tsDiff) >= 150)
            {
                throw new InvalidOperationException("Alexa Request Invalid: Request Timestamp outside valid range");
            }

            Request.Headers.TryGetValues("SignatureCertChainUrl", out var certUrls);
            Request.Headers.TryGetValues("Signature", out var signatures);

            var certChainUrl = certUrls.FirstOrDefault();
            var signature = signatures.FirstOrDefault();

            if (string.IsNullOrEmpty(certChainUrl))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing SignatureCertChainUrl header");
            }

            if (string.IsNullOrEmpty(signature))
            {
                throw new InvalidOperationException("Alexa Request Invalid: missing Signature header");
            }

            var uri = new Uri(certChainUrl);

            if (uri.Scheme.ToLower() != "https")
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad scheme");
            }

            if (uri.Port != 443)
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad port");
            }

            if (uri.Host.ToLower() != "s3.amazonaws.com")
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad host");
            }

            if (!uri.AbsolutePath.StartsWith("/echo.api/"))
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl bad path");
            }

            X509Certificate2 signingCertificate = null;

            if (!_validatedCertificateChains.ContainsKey(uri.ToString()))
            {
                System.Diagnostics.Trace.WriteLine("Validating cert URL: " + certChainUrl);

                var certList = await DownloadPemCertificatesAsync(uri.ToString());

                if (certList == null || certList.Length < 2)
                {
                    isValidated = false;
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl download failed or too few certificates");
                }

                var primaryCert = certList[0];

                var subjectAlternativeNameList = ParseSubjectAlternativeNames(primaryCert);

                if (!subjectAlternativeNameList.Contains("echo-api.amazon.com"))
                {
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate missing echo-api.amazon.com from Subject Alternative Names");
                }

                List<X509Certificate2> chainCerts = new List<X509Certificate2>();

                for (int i = 1; i < certList.Length; i++)
                {
                    chainCerts.Add(certList[i]);
                }

                if (!ValidateCertificateChain(primaryCert, chainCerts))
                {
                    throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate chain validation failed");
                }
                
                signingCertificate = primaryCert;

                lock (_validatedCertificateChains)
                {
                    if (!_validatedCertificateChains.ContainsKey(uri.ToString()))
                    {
                        System.Diagnostics.Trace.WriteLine("Adding validated cert url: " + uri.ToString());
                        _validatedCertificateChains[uri.ToString()] = primaryCert;
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Race condition hit while adding validated cert url: " + uri.ToString());
                    }
                }
            }
            else
            {
                signingCertificate = _validatedCertificateChains[uri.ToString()];
            }

            if (signingCertificate == null)
            {
                throw new InvalidOperationException("Alexa Request Invalid: SignatureCertChainUrl certificate generic failure");
            }


            var signatureBytes = Convert.FromBase64String(signature);
            var thing = signingCertificate.GetRSAPublicKey();

            //get the rawData as a byte array
            string rawData = string.Empty;
            byte[] requestBytes = null;
            using (var contentStream = await Request.Content.ReadAsStreamAsync())
            {
                contentStream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(contentStream))
                {
                    rawData = sr.ReadToEnd();
                }
            }
            try
            {
                requestBytes = Encoding.UTF8.GetBytes(rawData);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            //compare the hashs of the signature and the requestBody
            if (!thing.VerifyData(requestBytes, signatureBytes, System.Security.Cryptography.HashAlgorithmName.SHA1, System.Security.Cryptography.RSASignaturePadding.Pkcs1))
            {
                throw new InvalidOperationException("Alexa Request Invalid: Signature verification failed");
            }
        }
    }
}

