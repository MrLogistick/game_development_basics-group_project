using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public Highscores highscores;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI totalTime;

    public GameObject highscorePanel;
    public TextMeshProUGUI highscoresText;

    public Button playButton;
    public Button highscoresButton;
    
    public Transform destructableScenery;

    public GameObject[] tanks;
    float gameTime = 0f;
    public float GameTime { get {return gameTime;} }

    public Material neutralMat;
    public Material enemyMat;
    public Material playerMat;

    bool inGame = false;
    public bool InGame { get {return inGame;} }

    bool keyDown;

    AudioSource player;
    public AudioClip pregameMusic;
    public AudioClip ingameMusic;

    void Start() {
        player = GetComponent<AudioSource>();
        player.clip = pregameMusic;
        player.Play();

        for (int i = 0; i < tanks.Length; i++) {
            var materialAccess = tanks[i].GetComponent<MaterialAccess>();

            foreach (MeshRenderer rend in materialAccess.meshes) {
                rend.material = neutralMat;
            }
        }

        timerText.text = "";
        totalTime.text = "";
        messageText.text = "Tankinator.";

        highscorePanel.SetActive(false);
        playButton.gameObject.SetActive(true);
        highscoresButton.gameObject.SetActive(true);

        SetTanksActive(false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            Application.Quit();
        }

        if (inGame) Ingame();
        else Pregame();
    }

    public void OnHighscores() {
        messageText.gameObject.SetActive(!messageText.gameObject.activeSelf);
        highscorePanel.SetActive(!highscorePanel.gameObject.activeSelf);

        string text = "";
        for (int i = 0; i < highscores.scores.Length; i++) {
            int seconds = highscores.scores[i];

            if (highscores.scores[i] == 0) text += "--:--\n";
            else text += string.Format("{0:00}:{1:00}\n", seconds / 60, seconds % 60);
        }
        highscoresText.text = text;
    }

    public void Play() {
        keyDown = true;
    }

    void Pregame() {
        // if (Input.anyKeyDown) {
        //     if (Input.GetMouseButton(0) ||
        //         Input.GetMouseButton(1) ||
        //         Input.GetMouseButton(2)) return;
            
        //     keyDown = true;
        // }
        
        if (keyDown) {
            keyDown = false;

            highscorePanel.SetActive(false);
            playButton.gameObject.SetActive(false);
            highscoresButton.gameObject.SetActive(false);

            messageText.gameObject.SetActive(false);

            totalTime.text = "";

            gameTime = 0;
            inGame = true;

            SetTanksActive(true);
            
            for (int i = 0; i < tanks.Length; i++) {
                tanks[i].SetActive(true);
            }

            for (int i = 0; i < destructableScenery.childCount; i++) {
                destructableScenery.GetChild(i).gameObject.SetActive(true);
            }

            player.clip = ingameMusic;
            player.Play();
        }
    }

    void Ingame() {
        gameTime += Time.deltaTime;
        int seconds = Mathf.RoundToInt(gameTime);

        timerText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);

        if (IsPlayerDead() || OneTankLeft()) {
            messageText.gameObject.SetActive(true);
            inGame = false;

            player.clip = pregameMusic;
            player.Play();

            if (IsPlayerDead()) {
                messageText.text = "Tankinator-ed.";
            }
            else {
                if (IsPlayerScratchless()) {
                    messageText.text = "Flawless Tankinator.";

                    timerText.text += $" - {tanks.Length - 1}";
                    seconds -= tanks.Length - 1;
                }
                else {
                    messageText.text = "Master Tankinator.";
                    timerText.text += " - 0";
                }

                totalTime.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);

                highscores.AddScore(seconds);
                highscores.SaveScoresToFile();
            }

            playButton.gameObject.SetActive(true);
            highscoresButton.gameObject.SetActive(true);

            SetTanksActive(false);
        }
    }

    void SetTanksActive(bool active) {
        for (int i = 0; i < tanks.Length; i++) {
            var tank = tanks[i];
            var materialAccess = tank.GetComponent<MaterialAccess>();

            foreach (MeshRenderer rend in materialAccess.meshes) {
                if (tank.CompareTag("Player")) {
                    rend.material = active ? playerMat : neutralMat;
                    tank.GetComponent<TankMovement>().enabled = active;
                    tank.GetComponent<TankAim>().enabled = active;
                    tank.GetComponent<TankShooting>().enabled = active;
                }
                else {
                    rend.material = active ? enemyMat : neutralMat;
                    tank.GetComponent<EnemyMovement>().enabled = active;
                    tank.GetComponent<EnemyShooting>().enabled = active;
                }

                tank.GetComponent<TankHealth>().enabled = active;
            }
        }
    }

    bool OneTankLeft() {
        int tankCount = 0;

        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].activeSelf) tankCount++;
        }

        return tankCount <= 1;
    }

    bool IsPlayerDead() {
        for (int i = 0; i < tanks.Length; i++) {
            if (!tanks[i].activeSelf && tanks[i].CompareTag("Player")) return true;
        }

        return false;
    }

    bool IsPlayerScratchless() {
        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].GetComponent<TankHealth>().scratchless
                && tanks[i].CompareTag("Player")) return true;
        }

        return false;
    }
}