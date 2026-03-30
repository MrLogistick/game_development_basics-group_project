using System;
using System.IO;
using UnityEngine;

public class Highscores : MonoBehaviour {
    public int[] scores = new int[10];
    string currentDirectory;

    public string scoreFileName = "highscores.txt";

    void Start() {
        currentDirectory = Application.dataPath;
        Debug.Log("Our current directory is: " + currentDirectory);

        LoadScoresFromFile();
    }

    # if UNITY_EDITOR
    void Update() {
        if (Input.GetKeyDown(KeyCode.F9)) {
            LoadScoresFromFile();
        }
        if (Input.GetKeyDown(KeyCode.F10)) {
            SaveScoresToFile();
        }
        if (Input.GetKeyDown(KeyCode.F8)) {
            for (int i = 0; i < scores.Length; i++) {
                scores[i] = 0;
            }

            Debug.Log("Highscores reset");

            SaveScoresToFile();
        }
    }
    # endif

    public void LoadScoresFromFile() {
        bool fileExists = File.Exists(currentDirectory + "\\" + scoreFileName);
        if (!fileExists) {
            Debug.Log($"The file {scoreFileName} does not exist.", this);
            return;
        }

        scores = new int[scores.Length];

        StreamReader fileReader;
        try {
            fileReader = new StreamReader(currentDirectory + "\\" + scoreFileName);
        }
        catch (Exception e) {
            Debug.Log(e.Message, this);
            return;
        }

        int scoreCount = 0;

        while (fileReader.Peek() != 0 && scoreCount < scores.Length) {
            string fileLine = fileReader.ReadLine();
            int readScore = -1;

            bool didParse = int.TryParse(fileLine, out readScore);

            if (didParse) {
                scores[scoreCount] = readScore;
            }
            else {
                Debug.Log($"Invalid line in scores file at {scoreCount}, using defaul value.", this);
                scores[scoreCount] = 0;
            }

            scoreCount++;
        }

        fileReader.Close();

        Debug.Log("Highscores loaded from " + scoreFileName);
    }

    public void SaveScoresToFile() {
        StreamWriter fileWriter = new StreamWriter(currentDirectory + "\\" + scoreFileName);

        for (int i = 0; i < scores.Length; i++) {
            fileWriter.WriteLine(scores[i]);
        }

        fileWriter.Close();

        Debug.Log("Highscores written to " + scoreFileName);
    }

    public void AddScore(int newScore) {
        int desiredIndex = -1;

        for (int i = 0; i < scores.Length; i++) {
            if (scores[i] > newScore || scores[i] == 0) {
                desiredIndex = i;
                break;
            }
        }

        if ( desiredIndex < 0) {
            Debug.Log("Score of " + newScore + " not high enough for scores list.", this);
            return;
        }

        for (int i = scores.Length - 1; i > desiredIndex; i--) {
            scores[i] = scores[i - 1];
        }

        scores[desiredIndex] = newScore;
        Debug.Log($"Score of {newScore} entered into highscores at position {desiredIndex}", this);
    }
}