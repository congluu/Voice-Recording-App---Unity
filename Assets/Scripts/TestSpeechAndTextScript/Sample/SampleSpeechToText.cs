using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Android;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

public class SampleSpeechToText : MonoBehaviour
{
    //string[] keywordTestArr = { "Hihi", "Hello", "Hella", "Helli", "Hellu", "Helly" };
    //string[] keywordTestArr = { "Apple Read Well", "Appel Read Well", "Appell Read Well", "Apelles Read Well" };
    //string targetVocab = "Carrot cake with sambal chilli";
    List<Keywords> keywordsList;
    List<Questions> questionsList0;
    List<Questions> questionsList1;
    List<Questions> questionsList2;
    List<Questions> questionsList3;

    public GameObject loading;
    public InputField inputLocale;

    public InputField inputText;

    public float pitch;
    public float rate;
    public Slider pitchSlider;
    public Slider rateSlider;

    public Text txtLocale;
    public Text txtPitch;
    public Text txtRate;
    string langCode;

    public Text word1Text;
    public Text word2Text;
    public Text word3Text;
    public GameObject wordButton1;
    public GameObject wordButton2;
    public GameObject wordButton3;
    public Text showquestion_Text;

    public Text txtLog;
    public AudioSource audioSourceTest;

    void Start()
    {
        Setting("en-US");
        loading.SetActive(false);
        SpeechToText.instance.onResultCallback = OnResultSpeech;

        CheckPermission();

        ImplementKeywordsAndQuestionsData(); //Test Game Mechanics
    }


    void CheckPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    public void StartRecording()
    {
#if UNITY_EDITOR
        OnResultSpeech("Not support in editor.");
#else
        SpeechToText.instance.StartRecording("Speak any");

        //start recording 
        audioSourceTest.clip = Microphone.Start(Microphone.devices[0].ToString(), false, 2, 44100); //Set the audio clip using microphone record audio
        iPhoneSpeaker.ForceToSpeaker();
        txtLog.text += "Start Normal Recording!!!\n";
#endif
    }

    public void StopRecording()
    {
#if UNITY_EDITOR
        OnResultSpeech("Not support in editor.");
#else
        SpeechToText.instance.StopRecording();
#endif
#if UNITY_IOS
        loading.SetActive(true);
        Microphone.End(null); //stop recording
        AudioClipSerializer.SaveAudioClipToDisk(audioSourceTest.clip, "test"); //Save the audio clip that just record to local data
        txtLog.text += "Stop Normal Recording!!!\n";
       
#endif
        matchVoiceResultWithKeyword(); //Start analyze result and questions 
    }
    void OnResultSpeech(string _data)
    {
        inputText.text = _data;
#if UNITY_IOS
        loading.SetActive(false);
#endif
    }
    public void OnClickSpeak()
    {
        TextToSpeech.instance.StartSpeak(inputText.text);
    }
    public void  OnClickStopSpeak()
    {
        TextToSpeech.instance.StopSpeak();
    }

    void OnFinalSpeechResult(string result)
    {
        inputText.text = result;

        /*Compare result text through regex match to see it fit to keyword or not*/
        //testRegexFunction();

        /*Compare result text through Levenshtein Distance Algorithm to see it fit to keyword or not*/
        //testAlgorithm();
    }

    #region Try apply game mechanics
    void ImplementKeywordsAndQuestionsData() {
        keywordsList = new List<Keywords>();
        foreach (string key in KeywordsData.keyword5AllGreenData) {
            Keywords keywords = new Keywords();
            keywords.keyword = key;
            keywords.isExist = false;
            keywordsList.Add(keywords);
        }

        questionsList0 = new List<Questions>();
        foreach (string quest in QuestionsData.questionTarget0)
        {
            Questions question = new Questions();
            question.question = quest;
            question.isUsed = false;
            questionsList0.Add(question);
        }

        questionsList1 = new List<Questions>();
        foreach (string quest in QuestionsData.questionTarget1)
        {
            Questions question = new Questions();
            question.question = quest;
            question.isUsed = false;
            questionsList1.Add(question);
        }

        questionsList2 = new List<Questions>();
        foreach (string quest in QuestionsData.questionTarget2)
        {
            Questions question = new Questions();
            question.question = quest;
            question.isUsed = false;
            questionsList2.Add(question);
        }

        questionsList3 = new List<Questions>();
        foreach (string quest in QuestionsData.questionTarget3)
        {
            Questions question = new Questions();
            question.question = quest;
            question.isUsed = false;
            questionsList3.Add(question);
        }

        showquestion_Text.text = ""; //Clear text
    }

    bool isWordPresent(string sentence, string keyword)
    {
        // To break the sentence in words 
        string[] sens = sentence.ToLower().Split(' ');
        string[] keys = keyword.ToLower().Split(' ');

        // To temporarily store each individual word  
        foreach (string s in sens)
        {
            // Comparing the current word 
            // with the word to be searched 
            foreach (string k in keys) {
                if (s.CompareTo(k) == 0)
                {
                    return true;
                }
            }               
        }
        return false;
    }

    public void matchVoiceResultWithKeyword() {
        //Step 1: Put voice result to check with Keyword
        string voiceWordResult = inputText.text; //Example

        foreach (Keywords key in keywordsList)
        {
            if (isWordPresent(voiceWordResult, key.keyword)) //True
            {
                key.isExist = true; //Set to true if it contain 
                Debug.Log("Change " + key.keyword + " in List to true");               
            }
        }

        //**********Debug*********/
        foreach (Keywords key in keywordsList)
        {
            Debug.Log("Key: " + key.keyword + " Value: " + key.isExist);
        }
        //**********Debug*********/
        
        //Step 2: Analyze the question
        if (!validateAllKeywordsIsExisted())
        {
            analyzeWhichQuestionToShow();
        }
        else {
            showquestion_Text.text = ""; //Clear text
        }

        //Step 3: Handle the yes/no question
    }

    void analyzeWhichQuestionToShow()
    {
        int indexInKeywordsList = 0;
        foreach (Keywords key in keywordsList)
        {
            if (!key.isExist)
            {
                Debug.Log("KeyToBreak: " + key.keyword + " ValueToBreak: " + key.isExist + " then Break");
                indexInKeywordsList = keywordsList.IndexOf(key);
                break;
            }
        }
        Debug.Log("Index " + indexInKeywordsList.ToString() + " in KeywordsList");

        if (!iteratingThroughQuestionsList0(indexInKeywordsList))
        {
            Questions[] questions = questionsList0.ToArray();
            showquestion_Text.text = questions[indexInKeywordsList].question.ToString(); //Show Question
            questions[indexInKeywordsList].isUsed = true; //Question used!
            //Debug.Log("Go here");
        }
        else if (!iteratingThroughQuestionsList1(indexInKeywordsList))
        {
            Questions[] questions = questionsList1.ToArray();
            showquestion_Text.text = questions[indexInKeywordsList].question.ToString(); //Show Question
            questions[indexInKeywordsList].isUsed = true; //Question used!
            //Debug.Log("Go here 1");
        }
        else if (!iteratingThroughQuestionsList2(indexInKeywordsList))
        {
            Questions[] questions = questionsList2.ToArray();
            showquestion_Text.text = questions[indexInKeywordsList].question.ToString(); //Show Question
            questions[indexInKeywordsList].isUsed = true; //Question used!
            //Debug.Log("Go here 2");
        }
        else //Question List 3
        {
            Questions[] questions = questionsList3.ToArray();
            showquestion_Text.text = questions[indexInKeywordsList].question.ToString(); //Show Question
            questions[indexInKeywordsList].isUsed = true; //Question used!
            //Debug.Log("Go here 3");
        }
    }

    bool iteratingThroughQuestionsList0(int index)
    {
        Questions[] questions = questionsList0.ToArray();
        if (questions[index].isUsed)
        {
            return true;
        }
        else {
            return false;
        }       
    }
    bool iteratingThroughQuestionsList1(int index)
    {
        Questions[] questions = questionsList1.ToArray();
        if (questions[index].isUsed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool iteratingThroughQuestionsList2(int index)
    {
        Questions[] questions = questionsList2.ToArray();
        if (questions[index].isUsed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool validateAllKeywordsIsExisted() {
        foreach (Keywords key in keywordsList)
        {
            if (key.isExist)
            {
                continue;
            }
            else {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Apply Levenshtein Distance
    /*Apply Levenshtein Distance Algorithm*/
    //public void testAlgorithm() {
    //    string voiceWordResult;
    //    voiceWordResult = inputText.text;
    //    int MAXIMUM_VALUES_CAN_GET = 3; //Maximum values inside matchedStrings 
    //    float SIMILAR_PERCENTAGE_FIXED = 0.8f; //Max is 1 mean 100%

    //    //Step 1: Create list words that use for recommend
    //    List<string> matchedStrings = new List<string>();

    //    //Step 2: Go though all keyword (hard code) and compare with voiceWordResult using LevenshteinDistanceAlgorithm
    //    foreach (string recommendedWord in keywordTestArr)
    //    {
    //        //int changesNumber = 0; // < 3 mean similar words
    //        //changesNumber = StringDistance.LevenshteinDistance(voiceWordResult, recommendedWord);
    //        //Debug.Log("resultText: " + voiceWordResult + " recommendedWord: " + recommendedWord + " changesNumber: " + changesNumber);

    //        double similarPercentage = 0; // 0.6 ~ 1 mean similar

    //        similarPercentage = StringDistance.CalculateSimilarity(voiceWordResult, recommendedWord);
    //        Debug.Log("similar Percentage: " + similarPercentage);
    //        //Step 3: Add words that almost similar with voiceWordResult to list words in (step 1)
    //        if (similarPercentage <= SIMILAR_PERCENTAGE_FIXED) 
    //        {
    //            matchedStrings.Add(recommendedWord); 

    //            //Validation - Maximum 3 values inside matchedStrings 
    //            if (matchedStrings.Count >= MAXIMUM_VALUES_CAN_GET)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    //Validation & show Word Recommended through button
    //    if (matchedStrings.Count >= 3)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(true);
    //        wordButton3.SetActive(true);

    //        word1.text = matchedStrings[0];
    //        word2.text = matchedStrings[1];
    //        word3.text = matchedStrings[2];
    //    }
    //    else if (matchedStrings.Count == 2)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(true);
    //        wordButton3.SetActive(false);

    //        word1.text = matchedStrings[0];
    //        word2.text = matchedStrings[1];
    //    }
    //    else if (matchedStrings.Count == 1)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(false);
    //        wordButton3.SetActive(false);

    //        word1.text = matchedStrings[0];
    //    }
    //    else
    //    {
    //        wordButton1.SetActive(false);
    //        wordButton2.SetActive(false);
    //        wordButton3.SetActive(false);
    //    }
    //}

    /*Function when missing last word. Eg: App but user want is Apple*/
    //public void testRegexFunction() {
    //    string result;
    //    result = inputText.text;

    //    List<string> matchedStrings = new List<string>();
    //    foreach (string text in keywordTestArr)
    //    {
    //        if (Regex.IsMatch(text.ToLower(), $"^"+result.ToLower()))
    //        {
    //            matchedStrings.Add(text);

    //            //Validation - Maximum 3 values inside matchedStrings
    //            if (matchedStrings.Count >= 3) {
    //                break;
    //            }
    //        }
    //    }

    //    //Validation & show Word Recommended through button
    //    if (matchedStrings.Count >= 3)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(true);
    //        wordButton3.SetActive(true);

    //        word1.text = matchedStrings[0];
    //        word2.text = matchedStrings[1];
    //        word3.text = matchedStrings[2];
    //    }
    //    else if (matchedStrings.Count == 2)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(true);
    //        wordButton3.SetActive(false);

    //        word1.text = matchedStrings[0];
    //        word2.text = matchedStrings[1];
    //    }
    //    else if (matchedStrings.Count == 1)
    //    {
    //        wordButton1.SetActive(true);
    //        wordButton2.SetActive(false);
    //        wordButton3.SetActive(false);

    //        word1.text = matchedStrings[0];
    //    }
    //    else {
    //        wordButton1.SetActive(false);
    //        wordButton2.SetActive(false);
    //        wordButton3.SetActive(false);
    //    }
    //}
    #endregion

    void OnPartialSpeechResult(string result)
    {
        inputText.text = result;
    }

    #region Settings
    public void Setting(string code)
    {
        TextToSpeech.instance.Setting(code, pitch, rate);
        SpeechToText.instance.Setting(code);
        txtLocale.text = "Locale: " + code;
        txtPitch.text = "Pitch: " + pitch;
        txtRate.text = "Rate: " + rate;
        langCode = code;
    }
    public void OnClickApply()
    {
        Setting(inputLocale.text);
    }
    public void ChangePitch() {
        pitch = pitchSlider.value;
        TextToSpeech.instance.Setting(langCode, pitch, rate);
        txtPitch.text = "Pitch: " + pitch;
    }
    public void ChangeRate()
    {
        rate = rateSlider.value;
        TextToSpeech.instance.Setting(langCode, pitch, rate);
        txtRate.text = "Rate: " + rate;
    }
    public void ResetPitch()
    {
        pitch = 1;
        pitchSlider.value = 1;
        TextToSpeech.instance.Setting(langCode, pitch, rate);
        txtPitch.text = "Pitch: " + pitch;
    }
    public void ResetRate()
    {
        rate = 1;
        rateSlider.value = 1;
        TextToSpeech.instance.Setting(langCode, pitch, rate);
        txtRate.text = "Rate: " + rate;
    }

    #endregion

    #region Function support
    public void changeTextWhenClickButtonWordSuggestion(Text textInButton) {
        inputText.text = textInButton.text;
    }

    //For ClearLog button
    public void ClearLog()
    {
        txtLog.text = "";
    }

    //Play the audio file
    public void LoadAudio() {
        AudioClipSerializer.LoadAudioClipFromDisk(audioSourceTest, "test"); //Load the audio clip from local data
        audioSourceTest.Play(); //Play the audio clip
    }
    #endregion
}
