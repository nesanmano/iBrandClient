using System.Collections.Generic;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.Test.Common.Condition
{
    
    public class TestMultipleCondition : TestBaseNunit
    {
        /* // Ce test n'est plus valide car il utilise des sous-choix, qui n'existent plus
        /// Copié de la génération de code du projet DIA-2018-01
        [Test]
        public void TestMultipleConditionsFromCode()
        {
            var upperLeft = new Point(0, 0);
            
            var liveOverlays = new List<PreviewOverlayInfo>();

            // Pas mal plus rapide que de les faire à la main...
            for (var quote = 0; quote < 10; quote++)
            {
                for (var lg = 0; lg < 2; lg++)
                {
                    // Condition commune, on utilise la même référence.
                    var mCondition = new MultipleCondition
                    {
                        Conditions = new List<ICondition>
                        {
                            new QuestionCondition(1, 1, (lg + 1)),
                            new QuestionCondition(2, 1, (quote + 1))
                        }
                    };

                    // Live Preview
                    var prevInfo = new PreviewOverlayInfo($"file_{quote}_{lg}.png", upperLeft, null, mCondition);

                    liveOverlays.Add(prevInfo);
                }
            }

            // Liste de tests
            
            {
                var testRep = new MockRespondent
                {
                    MockAnswer = new List<Answer>
                    {
                        new Answer
                        {
                            QuestionNo = 1
                        },
                        new Answer
                        {
                            QuestionNo = 2
                        }
                    }
                };
                var testOverlayInfo = CheckForOverlay(liveOverlays, testRep);
                Assert.IsNotNull(testOverlayInfo);
                Assert.AreEqual("file_0_0.png", testOverlayInfo.FileName);
            }
            
            {
                var testRep = new MockRespondent
                {
                    MockAnswer = new List<Answer>
                    {
                        new Answer(-1, 1, 1, 2),
                        new Answer(-1, 2, 1, 1)
                    }
                };
                var testOverlayInfo = CheckForOverlay(liveOverlays, testRep);
                Assert.IsNotNull(testOverlayInfo);
                Assert.AreEqual("file_0_1.png", testOverlayInfo.FileName);
            }
            
            {
                var testRep = new MockRespondent
                {
                    MockAnswer = new List<Answer>
                    {
                        new Answer(-1, 1, 1, 1),
                        new Answer(-1, 2, 1, 9)
                    }
                };
                var testOverlayInfo = CheckForOverlay(liveOverlays, testRep);
                Assert.IsNotNull(testOverlayInfo);
                Assert.AreEqual("file_8_0.png", testOverlayInfo.FileName);
            }
            
            {
                var testRep = new MockRespondent
                {
                    MockAnswer = new List<Answer>
                    {
                        new Answer(-1, 1, 1, 2),
                        new Answer(-1, 2, 1, 10)
                    }
                };
                var testOverlayInfo = CheckForOverlay(liveOverlays, testRep);
                Assert.IsNotNull(testOverlayInfo);
                Assert.AreEqual("file_9_1.png", testOverlayInfo.FileName);
            }
        }
        */

        private static PreviewOverlayInfo CheckForOverlay(List<PreviewOverlayInfo> liveOverlays, IRespondent resp)
        {
            PreviewOverlayInfo found = null;
            foreach (var poi in liveOverlays)
            {
                var condition = poi.Condition;
                if (condition != null && !condition.Evaluate(resp)) continue;
                found = poi;
                break;
            }

            return found;
        }
    }
}