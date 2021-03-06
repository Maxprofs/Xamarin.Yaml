﻿namespace Yaml.Localization.Tests.Units
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Xamarin.Yaml.Localization;
    using Xamarin.Yaml.Localization.Configs;
    using Xamarin.Yaml.Localization.Exceptions;
    using Xamarin.Yaml.Parser;
    using Yaml.Localization.Locales;
    using Yaml.Localization.Tests.Models;

    [TestFixture]
    public class I18NTests
    {
        [Test]
        public async Task NotExistsLocale_ThrowNotFound()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            // Act & Assert
            Assert.ThrowsAsync<YamlTranslateException>(async () => { await I18N.Instance.ChangeLocale("fr"); });
        }

        [Test]
        public async Task GetAvailableCultures()
        {
            // Arrange & Act & Assert
            I18N.Initialize(new AssemblyContentConfig(new List<Assembly> {typeof(LocaleTests).Assembly})
            {
                ResourceFolder = "Locales"
            });
            Assert.GreaterOrEqual(I18N.Instance.GetAvailableCultures().Count(), 0);
            
            I18N.Initialize(new AssemblyContentConfig(new List<Assembly> {typeof(LocaleTests).Assembly})
            {
                ResourceFolder = "FullLocales"
            });
            Assert.GreaterOrEqual(I18N.Instance.GetAvailableCultures().Count(), 0);
        }
        
        [Test]
        public async Task NotExistsEmbeddedResource_ThrowNotFound()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(new List<Assembly> {typeof(LocaleTests).Assembly})
            {
                ResourceFolder = "BadLocales"
            });

            // Act & Assert
            Assert.ThrowsAsync<YamlTranslateException>(async () => { await I18N.Instance.ChangeLocale("en"); });
        }

        [Test]
        public async Task Detect_FullCultureName()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(new List<Assembly> {typeof(LocaleTests).Assembly})
            {
                ResourceFolder = "FullLocales"
            });

            // Act & Assert
            await I18N.Instance.ChangeLocale("en-US");
            await I18N.Instance.ChangeLocale("ru-RU");
        }

        [Test]
        public async Task Chech_OfflineLocale()
        {
            // Arrange
            var hostAssembly = this.GetType().Assembly;
            var offlineConfig = OfflineContentConfig.FromAssembly(hostAssembly, "en.yaml", "Locales");
            var remoteConfig = new RemoteContentConfig
            {
                CacheDir = Path.GetTempPath()
            };

            I18N.Initialize(remoteConfig, offlineConfig);

            // Act
            var offlineLocale = I18N.Instance.GetLocale("offline");
            await I18N.Instance.ChangeLocale(offlineLocale);
            var value = I18N.Instance.Translate("ViewModel.Locale");

            // Assert
            Assert.NotNull(offlineLocale);
            Assert.AreEqual("en", value);
        }

        [Test]
        public async Task Translate_SomeWords()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;

            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Locale");

            // Assert
            Assert.AreEqual("en", value);
        }

        [Test]
        public async Task Check_ThrowWhenKeyNotFound()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales",
                ParserConfig = new ParserConfig
                {
                    ThrowWhenKeyNotFound = true
                }
            });

            var friendlyLocale = I18N.Instance;

            await friendlyLocale.ChangeLocale("en");

            // Act & friendlyLocale.Translate("ViewModel.Locale.BadKey");
            Assert.Throws<KeyNotFoundException>(() => { friendlyLocale.Translate("ViewModel.Locale.BadKey"); });
        }
        
        [Test]
        public async Task Check_Logger()
        {
            // Arrange
            var traces = new List<string>();
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales",
                Logger = trace =>
                {
                    traces.Add(trace);
                }
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act & friendlyLocale.Translate("ViewModel.Locale.BadKey");
            Assert.GreaterOrEqual(traces.Count, 0);
        }

        [Test]
        public async Task Translate_SomeWordsWithChangeLocale()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;

            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Locale");
            Assert.AreEqual("en", value);
            await friendlyLocale.ChangeLocale("ru");
            value = friendlyLocale.Translate("ViewModel.Locale");

            // Assert
            Assert.AreEqual("ru", value);
        }

        [Test]
        public async Task Translate_SomeWordsWithFallback()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            friendlyLocale.FallbackLocale = "en";

            await friendlyLocale.ChangeLocale("fr");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Locale");

            // Assert
            Assert.AreEqual("en", value);
        }
        
        [Test]
        public async Task Translate_SomeWordsWithArgs()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Test1.Test2.TestArgs", 5);

            // Assert
            Assert.AreEqual("Any 5", value);
        }
        
        [Test]
        public async Task Translate_SomeObject()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.TranslateNamingFormat("ViewModel.Test1.Test2.TestObject", new {foo = "foo value", bar = "bar value"});

            // Assert
            Assert.AreEqual("Any foo value\nAny bar value", value);
        }
        
        [Test]
        public async Task Translate_WithoutSomeObject()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.TranslateNamingFormat("ViewModel.Test1.Test2.TestObject", new {foo = "foo value"});

            // Assert
            Assert.AreEqual("Any foo value\nAny {bar}", value);
        }
        
        [Test]
        public async Task Translate_SomeWordsWithArgs_WithoutArgs()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Locale", 5);

            // Assert
            Assert.AreEqual("en", value);
        }
        
        [Test]
        public async Task Translate_MultipleAssemblies()
        {
            // Arrange
            var hostAssembly = this.GetType().Assembly;
            I18N.Initialize(new AssemblyContentConfig(new List<Assembly> {hostAssembly, typeof(LocaleTests).Assembly})
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;
            await friendlyLocale.ChangeLocale("en");

            // Act
            var value = friendlyLocale.Translate("ViewModel.Locale");

            // Assert
            Assert.AreEqual("en", value);
        }

        [Test]
        public async Task Translate_Enum()
        {
            // Arrange
            I18N.Initialize(new AssemblyContentConfig(this.GetType().Assembly)
            {
                ResourceFolder = "Locales"
            });

            var friendlyLocale = I18N.Instance;

            await friendlyLocale.ChangeLocale("en");

            // Act
            var valueCatEn = friendlyLocale.TranslateEnum(Animal.Cat);
            var valueDogEn = friendlyLocale.TranslateEnum(Animal.Dog);
            var valueMonkeyEn = friendlyLocale.TranslateEnum(Animal.Monkey);
            
            await friendlyLocale.ChangeLocale("ru");
            
            var valueCatRu = friendlyLocale.TranslateEnum(Animal.Cat);
            var valueDogRu = friendlyLocale.TranslateEnum(Animal.Dog);
            var valueMonkeyRu = friendlyLocale.TranslateEnum(Animal.Monkey);

            // Assert
            Assert.AreEqual("Cat Value", valueCatEn);
            Assert.AreEqual("Dog Value", valueDogEn);
            Assert.AreEqual("Monkey Value", valueMonkeyEn);
            
            Assert.AreEqual("Кот Значение", valueCatRu);
            Assert.AreEqual("Собака Значение", valueDogRu);
            Assert.AreEqual("Макака Значение", valueMonkeyRu);
        }
    }
}