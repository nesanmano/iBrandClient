using System.Reflection;
using NomadLogic.PrintStation;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NomadLogic.Test.PrintStation
{
    /// <summary>
    /// Classe d'initialisation pour tout le namespace.
    /// </summary>
    [SetUpFixture]
    public class RunningFeatureConfigTestsInit
    {
        [OneTimeSetUp]
        public void Init()
        {
            RunningFeatureConfig.GetInstance();
        }
    }

    [TestFixture]
    public class RunningFeatureConfigTests
    {
        /// <summary>
        /// Teste les Valeur par défaut de certaines configurations, les plus fréquentes.
        /// </summary>
        /// <param name="conf">La configuration à tester</param>
        /// <returns></returns>
        [TestCase(FeatureConfigEnum.IsAutomaticPrint, ExpectedResult = false)]
        [TestCase(FeatureConfigEnum.HasFrame, ExpectedResult = true)]
        [TestCase(FeatureConfigEnum.HasPrint, ExpectedResult = true)]
        [TestCase(FeatureConfigEnum.HasQuestionnaire, ExpectedResult = false)]
        public bool GetConfigurationBooleanTest(FeatureConfigEnum conf)
        {
            var rfg = RunningFeatureConfig.GetInstance();
            
            return rfg.GetConfiguration<bool>(conf);
        }
        
        /// <summary>
        /// Vérifie si quand on met à jour une valeur dans la configuration, cette dernière se modifie correctement.
        /// Pour les variables Booléennes.
        /// </summary>
        [Test]
        public void SetConfigurationBooleanTest()
        {
            var rfg = RunningFeatureConfig.GetInstance();

            // Valeur Booléenne
            Assert.IsTrue(rfg.GetConfiguration<bool>(FeatureConfigEnum.HasFrame));

            rfg.SetConfiguration(FeatureConfigEnum.HasFrame, false);
            Assert.IsFalse(rfg.GetConfiguration<bool>(FeatureConfigEnum.HasFrame));

            rfg.SetConfiguration(FeatureConfigEnum.HasFrame, false); // No-Op
            Assert.IsFalse(rfg.GetConfiguration<bool>(FeatureConfigEnum.HasFrame));

            rfg.SetConfiguration(FeatureConfigEnum.HasFrame, true); // Revient à la valeur initiale
            Assert.IsTrue(rfg.GetConfiguration<bool>(FeatureConfigEnum.HasFrame));
        }

        /// <summary>
        /// Vérifie si quand on met à jour une valeur dans la configuration, cette dernière se modifie correctement.
        /// Pour les variables entières.
        /// </summary>
        [Test]
        public void SetConfigurationIntTest()
        {
            var rfg = RunningFeatureConfig.GetInstance();

            // Valeur par défaut. (0 - 500)
            var def = rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth);
            Assert.AreEqual(80, def);

            rfg.SetConfiguration(FeatureConfigEnum.GallerySidePanelWidth, 100);
            Assert.AreEqual(100, rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth));

            rfg.SetConfiguration(FeatureConfigEnum.GallerySidePanelWidth, 100); // No-op
            Assert.AreEqual(100, rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth));

            // Revient valeur initiale
            rfg.SetConfiguration(FeatureConfigEnum.GallerySidePanelWidth, null);
            Assert.AreEqual(80, rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth));
            
            // Valeur trop petite 
            rfg.SetConfiguration(FeatureConfigEnum.GallerySidePanelWidth, -10);
            Assert.AreEqual(0, rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth)); // minimum

            // Valeur trop grande 
            rfg.SetConfiguration(FeatureConfigEnum.GallerySidePanelWidth, 501);
            Assert.AreEqual(500, rfg.GetConfiguration<int>(FeatureConfigEnum.GallerySidePanelWidth)); // minimum
        }

        /// <summary>
        /// Vérifie si quand on met à jour une valeur dans la configuration, cette dernière se modifie correctement.
        /// Pour les variables String.
        /// </summary>
        [Test]
        public void SetConfigurationStringTest()
        {
            var rfg = RunningFeatureConfig.GetInstance();

            // Valeur Booléenne
            var defaultValue = rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor);
            Assert.That(defaultValue, Is.Not.Null.Or.Empty);

            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, "ababab");
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), "ababab");

            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, "ababab"); // No-Op
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), "ababab");

            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, null); // Détruit la valeur (back to default)
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), defaultValue);

            // Test de "boxing"
            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, 45); // int vers string
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), "45");

            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, 45.1f); // float vers string
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), "45.1");

            rfg.SetConfiguration(FeatureConfigEnum.BackgroundColor, 45.0f); // float vers string
            Assert.AreEqual(rfg.GetConfiguration<string>(FeatureConfigEnum.BackgroundColor), "45");
        }

        /// <summary>
        /// Vérifie si quand on met à jour une valeur dans la configuration, cette dernière se modifie correctement.
        /// Pour les variables à virgule flottante.
        /// </summary>
        [Test]
        public void SetConfigurationFloatTest()
        {
            var rfg = RunningFeatureConfig.GetInstance();

            // Valeur par défaut. (0 - 500)
            var def = rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding);
            Assert.AreEqual(0.04f, def);

            rfg.SetConfiguration(FeatureConfigEnum.PicturePadding, 50.087);
            Assert.AreEqual(50.087, rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding));

            rfg.SetConfiguration(FeatureConfigEnum.PicturePadding, 50.087); // No-op
            Assert.AreEqual(50.087, rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding));

            // Revient valeur initiale
            rfg.SetConfiguration(FeatureConfigEnum.PicturePadding, null);
            Assert.AreEqual(def, rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding));

            // Valeur trop petite 
            rfg.SetConfiguration(FeatureConfigEnum.PicturePadding, -10);
            Assert.AreEqual(0, rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding)); // minimum

            // Valeur trop grande 
            rfg.SetConfiguration(FeatureConfigEnum.PicturePadding, 100);
            Assert.AreEqual(99, rfg.GetConfiguration<float>(FeatureConfigEnum.PicturePadding)); // minimum
        }

        /// <summary>
        /// Teste pour de nombreux cas de figures où il est interdit ou impossible d'assigner la valeur demandée à la configuration.
        /// Particulièrement des valeurs String dans des primitives numériques.
        /// </summary>
        [TestCase(FeatureConfigEnum.GalleryPicturePerRow, "baboune")] // string dans int
        [TestCase(FeatureConfigEnum.GallerySearchOffset, "0.05")] // float dans int
        public void InvalidValues(FeatureConfigEnum confEnum, object value)
        {
            var rfg = RunningFeatureConfig.GetInstance();
            Assert.Throws<TargetInvocationException>(() =>
            {
                rfg.SetConfiguration(confEnum, value);
            });
        }
    }
}