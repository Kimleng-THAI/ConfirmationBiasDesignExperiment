// ==========================================
// ExpectedVsActualBiasSystem.cs - Distinguishing Expected from Actual Responses
// ==========================================
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LSL;

public class ExpectedVsActualBiasSystem : MonoBehaviour
{
    public static ExpectedVsActualBiasSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // ==========================================
    // EXPECTED RESPONSE CLASSIFICATION
    // ==========================================
    public enum ExpectedBiasResponse
    {
        EXPECTED_CONFIRMATION,      // We expect confirmation bias based on article-statement link
        EXPECTED_DISCONFIRMATION,   // We expect disconfirmation seeking
        EXPECTED_NEUTRAL           // We expect neutral response
    }

    // ==========================================
    // ACTUAL RESPONSE CLASSIFICATION
    // ==========================================
    public enum ActualBiasResponse
    {
        ACTUAL_CONFIRMATION,        // Participant actually showed confirmation bias
        ACTUAL_DISCONFIRMATION,     // Participant actually sought disconfirmation
        ACTUAL_NEUTRAL,            // Participant actually showed neutral response
        UNDETERMINED              // Not yet measured
    }

    // ==========================================
    // RESPONSE ALIGNMENT
    // ==========================================
    public enum ResponseAlignment
    {
        ALIGNED,                   // Actual matches expected
        UNEXPECTED_CONFIRMATION,   // Expected neutral/disconfirm but got confirmation
        UNEXPECTED_DISCONFIRMATION, // Expected confirm/neutral but got disconfirmation
        UNEXPECTED_NEUTRAL,        // Expected confirm/disconfirm but got neutral
        PENDING                    // Actual response not yet measured
    }

    [System.Serializable]
    public class BiasExpectationEvent
    {
        // Article selection information
        public string timestamp;
        public string articleCode;
        public string primaryStatementCode;
        public string articleType; // "confirmatory", "disconfirmatory", "neutral"

        // Phase 1 baseline
        public int phase1StatementRating; // 1-5 scale from Phase 1

        // EXPECTED response (based on framework)
        public ExpectedBiasResponse expectedResponse;
        public float expectedStrength; // How strong we expect the bias to be
        public string expectedRationale; // Why we expect this response

        // ACTUAL response (measured during/after article reading)
        public ActualBiasResponse actualResponse = ActualBiasResponse.UNDETERMINED;
        public int phase2ArticleRating; // 1-5 rating after reading article
        public float readingTime;
        public float scrollDepth;
        public List<string> eegMarkers; // Neural markers during reading

        // Comparison
        public ResponseAlignment alignment = ResponseAlignment.PENDING;
        public float surpriseScore; // How unexpected was the actual response
    }

    private Dictionary<string, ArticleStatementRelationship> articleRelationships;
    private Dictionary<string, int> phase1Responses; // Statement code -> rating
    private List<BiasExpectationEvent> expectationEvents = new List<BiasExpectationEvent>();

    // ==========================================
    // CALCULATE EXPECTED RESPONSE (At Article Selection)
    // ==========================================
    public BiasExpectationEvent CalculateExpectedResponse(string articleCode)
    {
        var article = articleRelationships[articleCode];
        int phase1Rating = phase1Responses[article.linkedStatementCode];

        var expectationEvent = new BiasExpectationEvent
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            articleCode = articleCode,
            primaryStatementCode = article.linkedStatementCode,
            articleType = article.articleType,
            phase1StatementRating = phase1Rating,
            eegMarkers = new List<string>()
        };

        // DETERMINE EXPECTED RESPONSE
        expectationEvent.expectedResponse = DetermineExpectedResponse(article.articleType, phase1Rating);
        expectationEvent.expectedStrength = CalculateExpectedStrength(phase1Rating);
        expectationEvent.expectedRationale = GenerateExpectationRationale(article.articleType, phase1Rating);

        // Send LSL marker for EXPECTED response
        SendExpectedResponseMarker(expectationEvent);

        // Store for later comparison with actual
        expectationEvents.Add(expectationEvent);

        return expectationEvent;
    }

    private ExpectedBiasResponse DetermineExpectedResponse(string articleType, int phase1Rating)
    {
        // Based on your framework:
        // We EXPECT confirmation bias when:
        // - Article is confirmatory AND participant agreed (rating ≥ 4)
        // - Article is disconfirmatory AND participant disagreed (rating ≤ 2)

        if (articleType == "confirmatory" && phase1Rating >= 4)
        {
            return ExpectedBiasResponse.EXPECTED_CONFIRMATION;
        }
        else if (articleType == "disconfirmatory" && phase1Rating <= 2)
        {
            return ExpectedBiasResponse.EXPECTED_CONFIRMATION;
        }
        // We EXPECT disconfirmation seeking when:
        // - Article is confirmatory AND participant disagreed (rating ≤ 2)
        // - Article is disconfirmatory AND participant agreed (rating ≥ 4)
        else if (articleType == "confirmatory" && phase1Rating <= 2)
        {
            return ExpectedBiasResponse.EXPECTED_DISCONFIRMATION;
        }
        else if (articleType == "disconfirmatory" && phase1Rating >= 4)
        {
            return ExpectedBiasResponse.EXPECTED_DISCONFIRMATION;
        }
        // We EXPECT neutral response when:
        // - Article is neutral type
        // - Participant had neutral stance (rating = 3)
        else
        {
            return ExpectedBiasResponse.EXPECTED_NEUTRAL;
        }
    }

    private float CalculateExpectedStrength(int phase1Rating)
    {
        // Stronger expected response for more extreme Phase 1 ratings
        return Mathf.Abs(phase1Rating - 3) / 2f; // 0-1 scale
    }

    private string GenerateExpectationRationale(string articleType, int phase1Rating)
    {
        if (articleType == "confirmatory" && phase1Rating >= 4)
            return $"Confirmatory article with prior agreement (rating={phase1Rating})";
        else if (articleType == "disconfirmatory" && phase1Rating <= 2)
            return $"Disconfirmatory article with prior disagreement (rating={phase1Rating})";
        else if (articleType == "confirmatory" && phase1Rating <= 2)
            return $"Confirmatory article with prior disagreement (rating={phase1Rating})";
        else if (articleType == "disconfirmatory" && phase1Rating >= 4)
            return $"Disconfirmatory article with prior agreement (rating={phase1Rating})";
        else if (articleType == "neutral")
            return "Neutral article type";
        else
            return $"Neutral prior stance (rating={phase1Rating})";
    }

    // ==========================================
    // RECORD ACTUAL RESPONSE (During/After Article Reading)
    // ==========================================
    public void RecordActualResponse(string articleCode, int phase2Rating, float readingTime, float scrollDepth)
    {
        var expectationEvent = expectationEvents.LastOrDefault(e => e.articleCode == articleCode);
        if (expectationEvent == null)
        {
            Debug.LogError($"No expected response found for article {articleCode}");
            return;
        }

        // Store actual behavioral data
        expectationEvent.phase2ArticleRating = phase2Rating;
        expectationEvent.readingTime = readingTime;
        expectationEvent.scrollDepth = scrollDepth;

        // DETERMINE ACTUAL RESPONSE
        expectationEvent.actualResponse = DetermineActualResponse(
            expectationEvent.phase1StatementRating,
            phase2Rating,
            readingTime,
            expectationEvent.articleType
        );

        // COMPARE EXPECTED VS ACTUAL
        expectationEvent.alignment = CompareExpectedVsActual(
            expectationEvent.expectedResponse,
            expectationEvent.actualResponse
        );

        // Calculate surprise score
        expectationEvent.surpriseScore = CalculateSurpriseScore(expectationEvent);

        // Send comprehensive LSL markers
        SendActualResponseMarkers(expectationEvent);

        // Log the comparison
        LogExpectedVsActual(expectationEvent);
    }

    private ActualBiasResponse DetermineActualResponse(int phase1Rating, int phase2Rating, float readingTime, string articleType)
    {
        // ACTUAL confirmation bias indicators:
        // - High agreement with confirmatory content (phase2 ≥ 4 for confirmatory articles)
        // - Quick dismissal of disconfirmatory content (short reading time + low rating)
        // - Rating shift towards article stance

        int ratingShift = phase2Rating - phase1Rating;

        if (articleType == "confirmatory")
        {
            if (phase2Rating >= 4 && phase1Rating >= 4)
            {
                // Maintained or strengthened agreement
                return ActualBiasResponse.ACTUAL_CONFIRMATION;
            }
            else if (phase2Rating <= 2 && phase1Rating >= 4)
            {
                // Changed mind against prior belief
                return ActualBiasResponse.ACTUAL_DISCONFIRMATION;
            }
        }
        else if (articleType == "disconfirmatory")
        {
            if (phase2Rating <= 2 && phase1Rating <= 2)
            {
                // Maintained disagreement despite disconfirmatory evidence
                return ActualBiasResponse.ACTUAL_CONFIRMATION;
            }
            else if (phase2Rating >= 4 && phase1Rating <= 2)
            {
                // Changed mind based on disconfirmatory evidence
                return ActualBiasResponse.ACTUAL_DISCONFIRMATION;
            }
        }

        // Check for quick dismissal (confirmation bias indicator)
        if (readingTime < 10f && Mathf.Abs(ratingShift) < 1)
        {
            return ActualBiasResponse.ACTUAL_CONFIRMATION;
        }

        // Default to neutral if no clear pattern
        return ActualBiasResponse.ACTUAL_NEUTRAL;
    }

    private ResponseAlignment CompareExpectedVsActual(ExpectedBiasResponse expected, ActualBiasResponse actual)
    {
        // Check if actual response aligns with expected
        if (expected == ExpectedBiasResponse.EXPECTED_CONFIRMATION &&
            actual == ActualBiasResponse.ACTUAL_CONFIRMATION)
        {
            return ResponseAlignment.ALIGNED;
        }
        else if (expected == ExpectedBiasResponse.EXPECTED_DISCONFIRMATION &&
                 actual == ActualBiasResponse.ACTUAL_DISCONFIRMATION)
        {
            return ResponseAlignment.ALIGNED;
        }
        else if (expected == ExpectedBiasResponse.EXPECTED_NEUTRAL &&
                 actual == ActualBiasResponse.ACTUAL_NEUTRAL)
        {
            return ResponseAlignment.ALIGNED;
        }

        // Determine type of misalignment
        if (actual == ActualBiasResponse.ACTUAL_CONFIRMATION)
        {
            return ResponseAlignment.UNEXPECTED_CONFIRMATION;
        }
        else if (actual == ActualBiasResponse.ACTUAL_DISCONFIRMATION)
        {
            return ResponseAlignment.UNEXPECTED_DISCONFIRMATION;
        }
        else
        {
            return ResponseAlignment.UNEXPECTED_NEUTRAL;
        }
    }

    private float CalculateSurpriseScore(BiasExpectationEvent evt)
    {
        // How surprising is the actual response given our expectation?
        // 0 = perfectly aligned, 1 = completely unexpected

        if (evt.alignment == ResponseAlignment.ALIGNED)
        {
            return 0f;
        }
        else if (evt.alignment == ResponseAlignment.UNEXPECTED_CONFIRMATION &&
                 evt.expectedResponse == ExpectedBiasResponse.EXPECTED_DISCONFIRMATION)
        {
            // Very surprising - opposite of expected
            return 1f;
        }
        else if (evt.alignment == ResponseAlignment.UNEXPECTED_DISCONFIRMATION &&
                 evt.expectedResponse == ExpectedBiasResponse.EXPECTED_CONFIRMATION)
        {
            // Very surprising - opposite of expected
            return 1f;
        }
        else
        {
            // Moderate surprise
            return 0.5f;
        }
    }

    // ==========================================
    // LSL MARKERS - CLEARLY DISTINGUISHING EXPECTED VS ACTUAL
    // ==========================================
    private void SendExpectedResponseMarker(BiasExpectationEvent evt)
    {
        // CLEARLY MARK AS EXPECTED
        string marker = $"EXPECTED_BIAS_{evt.articleCode}_" +
                       $"{evt.expectedResponse}_" +
                       $"STRENGTH{evt.expectedStrength:F2}_" +
                       $"PHASE1_R{evt.phase1StatementRating}";

        LSLManager.Instance.SendMarker(marker);

        // Also send rationale for debugging
        LSLManager.Instance.SendMarker($"EXPECTED_RATIONALE_{evt.expectedRationale}");
    }

    private void SendActualResponseMarkers(BiasExpectationEvent evt)
    {
        // CLEARLY MARK AS ACTUAL
        string actualMarker = $"ACTUAL_BIAS_{evt.articleCode}_" +
                            $"{evt.actualResponse}_" +
                            $"PHASE2_R{evt.phase2ArticleRating}_" +
                            $"TIME{evt.readingTime:F1}";

        LSLManager.Instance.SendMarker(actualMarker);

        // Send alignment marker
        string alignmentMarker = $"BIAS_ALIGNMENT_{evt.articleCode}_" +
                               $"{evt.alignment}_" +
                               $"SURPRISE{evt.surpriseScore:F2}";

        LSLManager.Instance.SendMarker(alignmentMarker);

        // Send behavioral data
        int expectedNum = (int)evt.expectedResponse;
        int actualNum = (int)evt.actualResponse;

        LSLManager.Instance.SendBehavioralData(
            expectationEvents.Count,
            GetTopicID(evt.articleCode),
            GetArticleID(evt.articleCode),
            expectedNum,  // Use expected as bias type
            evt.scrollDepth,
            evt.readingTime
        );
    }

    // ==========================================
    // NEURAL MARKERS DURING READING
    // ==========================================
    public void RecordEEGMarkerDuringReading(string articleCode, string eegMarker)
    {
        var evt = expectationEvents.LastOrDefault(e => e.articleCode == articleCode);
        if (evt != null)
        {
            evt.eegMarkers.Add(eegMarker);

            // Send marker indicating neural activity during expected bias condition
            string neuralMarker = $"EEG_DURING_{evt.expectedResponse}_{eegMarker}";
            LSLManager.Instance.SendMarker(neuralMarker);
        }
    }

    // ==========================================
    // ANALYSIS AND REPORTING
    // ==========================================
    private void LogExpectedVsActual(BiasExpectationEvent evt)
    {
        string log = $"\n========== EXPECTED vs ACTUAL ANALYSIS ==========\n" +
                    $"Article: {evt.articleCode} ({evt.articleType})\n" +
                    $"Statement: {evt.primaryStatementCode}\n" +
                    $"Phase 1 Rating: {evt.phase1StatementRating}\n" +
                    $"EXPECTED: {evt.expectedResponse} (Strength: {evt.expectedStrength:F2})\n" +
                    $"ACTUAL: {evt.actualResponse}\n" +
                    $"Phase 2 Rating: {evt.phase2ArticleRating}\n" +
                    $"Reading Time: {evt.readingTime:F1}s\n" +
                    $"Alignment: {evt.alignment}\n" +
                    $"Surprise Score: {evt.surpriseScore:F2}\n" +
                    $"================================================\n";

        Debug.Log(log);
    }

    public ExpectationValidationReport GenerateValidationReport()
    {
        var report = new ExpectationValidationReport
        {
            totalEvents = expectationEvents.Count,
            alignedCount = expectationEvents.Count(e => e.alignment == ResponseAlignment.ALIGNED),
            unexpectedConfirmationCount = expectationEvents.Count(e => e.alignment == ResponseAlignment.UNEXPECTED_CONFIRMATION),
            unexpectedDisconfirmationCount = expectationEvents.Count(e => e.alignment == ResponseAlignment.UNEXPECTED_DISCONFIRMATION),
            unexpectedNeutralCount = expectationEvents.Count(e => e.alignment == ResponseAlignment.UNEXPECTED_NEUTRAL),
            averageSurpriseScore = expectationEvents.Average(e => e.surpriseScore),
            frameworkAccuracy = 0f
        };

        // Calculate framework accuracy (how often expected matched actual)
        if (report.totalEvents > 0)
        {
            report.frameworkAccuracy = (float)report.alignedCount / report.totalEvents;
        }

        // Identify patterns where framework predictions fail
        report.commonMisalignments = IdentifyMisalignmentPatterns();

        return report;
    }

    private List<string> IdentifyMisalignmentPatterns()
    {
        var patterns = new List<string>();

        // Check for systematic misalignments
        var misaligned = expectationEvents.Where(e => e.alignment != ResponseAlignment.ALIGNED);

        // Pattern 1: Confirmatory articles not inducing confirmation bias
        var failedConfirmatory = misaligned.Where(e =>
            e.expectedResponse == ExpectedBiasResponse.EXPECTED_CONFIRMATION &&
            e.actualResponse != ActualBiasResponse.ACTUAL_CONFIRMATION);

        if (failedConfirmatory.Count() > 2)
        {
            patterns.Add($"Confirmatory articles failed to induce expected bias in {failedConfirmatory.Count()} cases");
        }

        // Pattern 2: Unexpected confirmation bias in neutral conditions
        var unexpectedConfirmation = misaligned.Where(e =>
            e.expectedResponse == ExpectedBiasResponse.EXPECTED_NEUTRAL &&
            e.actualResponse == ActualBiasResponse.ACTUAL_CONFIRMATION);

        if (unexpectedConfirmation.Count() > 2)
        {
            patterns.Add($"Neutral articles unexpectedly induced confirmation bias in {unexpectedConfirmation.Count()} cases");
        }

        return patterns;
    }

    [System.Serializable]
    public class ExpectationValidationReport
    {
        public int totalEvents;
        public int alignedCount;
        public int unexpectedConfirmationCount;
        public int unexpectedDisconfirmationCount;
        public int unexpectedNeutralCount;
        public float averageSurpriseScore;
        public float frameworkAccuracy;
        public List<string> commonMisalignments;
    }

    // Helper functions
    private int GetTopicID(string articleCode)
    {
        return int.Parse(articleCode.Substring(1, 2));
    }

    private int GetArticleID(string articleCode)
    {
        int topic = GetTopicID(articleCode);
        char letter = articleCode[3];
        return topic * 100 + (letter - 'A' + 1);
    }

    [System.Serializable]
    public class ArticleStatementRelationship
    {
        public string articleCode;
        public string linkedStatementCode;
        public string articleType;
        public List<SecondaryRelationship> secondaryRelationships;
    }

    [System.Serializable]
    public class SecondaryRelationship
    {
        public string statementCode;
        public float weight;
    }
}