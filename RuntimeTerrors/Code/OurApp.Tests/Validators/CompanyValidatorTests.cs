using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Validators;
using System;

namespace OurApp.Tests.Validators
{
    [TestClass]
    public class CompanyValidatorTests
    {
        private CompanyValidator validator;

        [TestInitialize]
        public void Setup()
        {
            validator = new CompanyValidator();
        }

        
        //name
        [TestMethod]
        public void NameValidator_ValidName_ReturnsTrue()
        {
            string name = "Google";
            bool result = validator.NameValidator(name);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NameValidator_EmptyName_ThrowsException()
        {
            string name = "";
            Action action = () => validator.NameValidator(name);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void NameValidator_NameTooLong_ThrowsException()
        {
            string name = new string('a', 201);
            Action action = () => validator.NameValidator(name);
            Assert.ThrowsException<Exception>(action);
        }


        //about us
        [TestMethod]
        public void AboutUsValidator_EmptyAboutUs_ReturnsTrue()
        {
            string about = "";
            bool result = validator.AboutUsValidator(about);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void AboutUsValidator_TooLongAboutUs_ThrowsException()
        {
            
            string about = new string('a', 2001);
            Action action = () => validator.AboutUsValidator(about);
            Assert.ThrowsException<Exception>(action);
        }


        [TestMethod]
        public void AboutUsValidator_NullAboutUs_ReturnsTrue()
        {
            string about = null;
            bool result = validator.AboutUsValidator(about);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void AboutUsValidator_ValidAboutUs_ReturnsTrue()
        {
            string about = "We build software.";
            bool result = validator.AboutUsValidator(about);
            Assert.IsTrue(result);
        }


        //location
        [TestMethod]
        public void LocationValidator_EmptyLocation_ReturnsTrue()
        {
            string location = "";
            bool result = validator.LocationValidator(location);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LocationValidator_LocationTooLong_ThrowsException()
        {
            string location = new string('a', 301);
            Action action = () => validator.LocationValidator(location);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LocationValidator_NullLocation_ReturnsTrue()
        {
            string location = null;
            bool result = validator.LocationValidator(location);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LocationValidator_ValidLocation_ReturnsTrue()
        {
            string location = "Bucharest";
            bool result = validator.LocationValidator(location);
            Assert.IsTrue(result);
        }


        //email
        [TestMethod]
        public void EmailValidator_EmptyEmail_ReturnsTrue()
        {
            string email = "";
            bool result = validator.EmailValidator(email);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EmailValidator_EmailWithoutAt_ThrowsException()
        {
            string email = "invalidemail";
            Action action = () => validator.EmailValidator(email);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void EmailValidator_EmailTooLong_ThrowsException()
        { 
            string email = new string('a', 101) + "@gmail.com";
            Action action = () => validator.EmailValidator(email);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void EmailValidator_ValidEmail_ReturnsTrue()
        {
            string email = "test@gmail.com";
            bool result = validator.EmailValidator(email);
            Assert.IsTrue(result);
        }

       
        //profile picture
        [TestMethod]
        public void PfpValidator_EmptyPfp_ReturnsTrue()
        {
            string pfp = "";
            bool result = validator.PfpValidator(pfp);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PfpValidator_InvalidExtension_ThrowsException()
        {
            string pfp = "image.txt";
            Action action = () => validator.PfpValidator(pfp);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void PfpValidator_ValidImage_ReturnsTrue()
        {
            string pfp = "image.png";
            bool result = validator.PfpValidator(pfp);
            Assert.IsTrue(result);
        }


        //logo
        [TestMethod]
        public void LogoValidator_EmptyLogo_ThrowsException()
        {
            string logo = "";
            Action action = () => validator.LogoValidator(logo);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LogoValidator_InvalidExtension_ThrowsException()
        {
            string logo = "logo.txt";
            Action action = () => validator.LogoValidator(logo);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void LogoValidator_ValidLogo_ReturnsTrue()
        { 
            string logo = "logo.jpg";
            bool result = validator.LogoValidator(logo);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void PfpValidator_DataImagePng_ReturnsTrue()
        {
            string pfp = "data:image/png;base64,AAA";
            bool result = validator.PfpValidator(pfp);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void LogoValidator_DataImageJpeg_ReturnsTrue()
        {
            string logo = "data:image/jpeg;base64,AAA";
            bool result = validator.LogoValidator(logo);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void PfpValidator_WhitespaceImage_ThrowsException()
        { 
            string pfp = "   ";
            Action action = () => validator.PfpValidator(pfp);
            Assert.ThrowsException<Exception>(action);
        }
    }
}