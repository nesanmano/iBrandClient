using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using NUnit.Framework;

namespace NomadLogic.Test
{
    /// <summary>
    /// Ajout de fonctions pour initialiser, vérifier et gérer des objets pour Amazon Web Services.
    /// </summary>
    public abstract class TestBaseNunitAws : TestBaseNunit
    {
        protected const string AWS_DEFAULT_PROFILE_UNIT_TESTS = "aws_ibrand_unit_tests";
        protected static readonly RegionEndpoint AWS_DEFAULT_REGION = RegionEndpoint.APNortheast2;
        private const string AWS_DEFAULT_ACCESS_KEY = "AKIA6ESNZIUGKTLHXLUS";
        private const string AWS_DEFAULT_SECRET_KEY = "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";
        protected const string AWS_DEFAULT_REGION_ENDPOINT = "us-east-2";

        protected AWSCredentials AwsCredentials { get; private set; }

        /// <summary>
        /// Dans cette version de classes de tests unitaires, on configure et charge un fichier de "credentials" pour un compte AWS.
        /// </summary>
        [OneTimeSetUp]
        public override void InitConfigurationAndNlog()
        {
            base.InitConfigurationAndNlog();

            // Chargement du profil par défaut
            var sharedFile = new SharedCredentialsFile();
            if (!sharedFile.TryGetProfile(AWS_DEFAULT_PROFILE_UNIT_TESTS, out var profile))
            {
                CreateAwsCredentialsFile(AWS_DEFAULT_PROFILE_UNIT_TESTS, AWS_DEFAULT_REGION, AWS_DEFAULT_ACCESS_KEY, AWS_DEFAULT_SECRET_KEY, sharedFile);

                Assert.IsTrue(sharedFile.TryGetProfile(AWS_DEFAULT_PROFILE_UNIT_TESTS, out profile)); //  Écrase l'ancienne instance de profile.
            }

            Assert.IsTrue(AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var awsCreds));

            AwsCredentials = awsCreds;
            AWSConfigs.AWSRegion = AWS_DEFAULT_REGION_ENDPOINT;
        }

        protected void CreateAwsCredentialsFile(string profileName, RegionEndpoint region, string accessKey, string secretKey, SharedCredentialsFile initialShareFile = null)
        {
            var options = new CredentialProfileOptions
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };
            var profile = new CredentialProfile(profileName, options) {Region = region};
            var sharedFile = initialShareFile ?? new SharedCredentialsFile();
            sharedFile.RegisterProfile(profile);
        }
    }
}
