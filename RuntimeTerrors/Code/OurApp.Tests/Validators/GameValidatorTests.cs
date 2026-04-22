using Microsoft.VisualStudio.TestTools.UnitTesting;
using OurApp.Core.Validators;
using System;
using System.Collections.Generic;

namespace OurApp.Tests.Validators
{
    [TestClass]
    public class GameValidatorTests
    {
        private GameValidator validator;

        [TestInitialize]
        public void Setup()
        {
            validator = new GameValidator();
        }

        private List<(string scenarioText, IReadOnlyList<(string advice, string feedback)> choices)> GetValidScenarios()
        {
            return new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("First scenario", new List<(string, string)>
                    {
                        ("Advice 1", "Reaction 1"),
                        ("Advice 2", "Reaction 2")
                    }),

                ("Second scenario", new List<(string, string)>
                    {
                        ("Advice 3", "Reaction 3")
                    })
            };
        }


        // MandatoryFieldsValidator
        [TestMethod]
        public void MandatoryFieldsValidator_ValidScenarios_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            var result = validator.ValidateMandatoryFields(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NullScenarios_ThrowsException()
        {
            List<(string, IReadOnlyList<(string, string)>)> scenarios = null;
            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_ScenarioTextMissing_ThrowsException()
        {
            var scenarios = GetValidScenarios();
            scenarios[0] = ("", scenarios[0].choices);
            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NoChoices_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Scenario 1", new List<(string,string)>()),
                ("Scenario 2", new List<(string,string)>{("Advice","Reaction")})
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_AdviceMissing_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Scenario 1", new List<(string,string)>
                {
                    ("", "Reaction")
                }),

                ("Scenario 2", new List<(string,string)>
                {
                    ("Advice","Reaction")
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_FeedbackMissing_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
            ("Scenario 1", new List<(string,string)>
                {
                    ("Advice", "")
                }),
        
                ("Scenario 2", new List<(string,string)>
                {
                    ("Advice","Reaction")
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void MandatoryFieldsValidator_NotTwoScenarios_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Only one scenario", new List<(string,string)>
                {
                    ("Advice","Reaction")
                })
            };

            Action action = () => validator.ValidateMandatoryFields(scenarios);

            Assert.ThrowsException<Exception>(action);
        }


        //CharacterLimitsValidator
        [TestMethod]
        public void CharacterLimitsValidator_ValidScenario_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CharacterLimitsValidator_ScenarioTooLong_ThrowsException()
        {
            var scenarios = GetValidScenarios();
            scenarios[0] = (new string('a', 251), scenarios[0].choices);
            Action action = () => validator.ValidateCharacterLimits(scenarios);
            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void CharacterLimitsValidator_AdviceTooLong_ThrowsException()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Scenario 1", new List<(string,string)>
                {
                    (new string('a', 251), "Reaction")
                }),
            
                ("Scenario 2", new List<(string,string)>
                {
                    ("Advice","Reaction")
                })
            };

            Action action = () => validator.ValidateCharacterLimits(scenarios);

            Assert.ThrowsException<Exception>(action);
        }

        [TestMethod]
        public void CharacterLimitsValidator_NullScenarios_ReturnsTrue()
        {
            List<(string, IReadOnlyList<(string, string)>)> scenarios = null;
            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void CharacterLimitsValidator_NullChoices_ReturnsTrue()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                ("Scenario 1", null),
                ("Scenario 2", new List<(string,string)>{("Advice","Reaction")})
            };

            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void CharacterLimitsValidator_ScenarioAtLimit_ReturnsTrue()
        {
            var scenarios = new List<(string, IReadOnlyList<(string, string)>)>
            {
                (new string('a', GameValidator.MaxStruggleOrAdviceLength),
                 new List<(string,string)>
                 {
                    ("Advice","Reaction")
                 }),
        
                ("Scenario 2", new List<(string,string)>
                {
                    ("Advice","Reaction")
                })
            };

            var result = validator.ValidateCharacterLimits(scenarios);
            Assert.IsTrue(result);
        }


        //ConclusionPositiveValidator

        [TestMethod]
        public void ConclusionPositiveValidator_ValidConclusion_ReturnsTrue()
        {
            string conclusion = "Well done!";
            var result = validator.ValidatePositiveConclusion(conclusion);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConclusionPositiveValidator_EmptyConclusion_ThrowsException()
        {
            string conclusion = "";
            Action action = () => validator.ValidatePositiveConclusion(conclusion);
            Assert.ThrowsException<Exception>(action);
        }


        //ValidateForActivation
        [TestMethod]
        public void ValidateForActivation_ValidData_ReturnsTrue()
        {
            var scenarios = GetValidScenarios();
            string conclusion = "Good ending";
            var result = validator.ValidateForActivation(scenarios, conclusion);
            Assert.IsTrue(result);
        }
    }
}