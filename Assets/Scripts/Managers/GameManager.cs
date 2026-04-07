using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public Highscores timedHighscores;
    public Highscores endlessHighscores;

    public TextMeshProUGUI messageText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI totalTime;

    public GameObject highscorePanel;
    public TextMeshProUGUI highscoresText;

    public Button playButton;
    public Button endlessButton;
    public Button highscoresButton;
  
    public Button TankSelectionButton;

    public GameObject[] enemyTanks;
    public GameObject TankSelectionPanel;
    public GameObject LighTank;

    public GameObject MeduimTank;
    public GameObject HeavyTank;
  
    public Transform destructableScenery;

    public GameObject[] tanks;
    float gameTime = 0f;
    public float GameTime { get {return gameTime;} }
    int tanksDestroyed = 0;

    public Material neutralMat;
    public Material enemyMat;
    public Material playerMat;

    bool inGame = false;
    public bool InGame { get {return inGame;} }

    bool keyDown;
    bool endless;

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
        endlessButton.gameObject.SetActive(true);
        highscoresButton.gameObject.SetActive(true);
        TankSelectionButton.gameObject.SetActive(true);

        SetTanksActive(false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (inGame) {
                inGame = false;
                
                if (endless) Endless();
                else Ingame();
            }
            else {
                Application.Quit();
            }
        }

        if (inGame) {
            if (endless) Endless();
            else Ingame();
        }
        else Pregame();
    }

    public void OnHighscores() {
        messageText.gameObject.SetActive(!messageText.gameObject.activeSelf);
        highscorePanel.SetActive(!highscorePanel.gameObject.activeSelf);

        ViewHighscores();
    }

    void ViewHighscores() {
        string text = "";
        
        if (endless) {
            for (int i = 0; i < endlessHighscores.scores.Length; i++) {
                int score = endlessHighscores.scores[i];

                if (endlessHighscores.scores[i] == 0) text += "--\n";
                else text += string.Format("{0:00}\n", score);
            }
        }
        else {
            for (int i = 0; i < timedHighscores.scores.Length; i++) {
                int seconds = timedHighscores.scores[i];

                if (timedHighscores.scores[i] == 0) text += "--:--\n";
                else text += string.Format("{0:00}:{1:00}\n", seconds / 60, seconds % 60);
            }
        }
        
        highscoresText.text = text;
    }

    public void Play(bool endless) {
        this.endless = endless;

        if (!highscorePanel.activeSelf) keyDown = true;
        else ViewHighscores();
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
            endlessButton.gameObject.SetActive(false);
            highscoresButton.gameObject.SetActive(false);
            TankSelectionButton.gameObject.SetActive(false);

            messageText.gameObject.SetActive(false);

            totalTime.text = "";

            gameTime = 0;
            inGame = true;

            SetTanksActive(true);
            
            for (int i = 0; i < tanks.Length; i++) {
                if (i == selectedTankIndex)
                {
                    tanks[i].tag = "Player";
                    tanks[i].SetActive(true);
                }
                else
                {
                    tanks[i].tag = "Untagged";
                    tanks[i].SetActive(false);
                }
            }

            foreach (GameObject enemy in enemyTanks)
            {
                enemy.SetActive(true);
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

        if (IsPlayerDead() || TanksLeft() <= 1 || inGame == false) {
            messageText.gameObject.SetActive(true);

            player.clip = pregameMusic;
            player.Play();

            if (inGame == false) {
                timerText.text = "";
                totalTime.text = "";
                messageText.text = "Tankinator.";
            }
            else if (IsPlayerDead()) {
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

                timedHighscores.AddScore(seconds);
                timedHighscores.SaveScoresToFile();
            }

            playButton.gameObject.SetActive(true);
            endlessButton.gameObject.SetActive(true);
            highscoresButton.gameObject.SetActive(true);
            TankSelectionButton.gameObject.SetActive(true);

            SetTanksActive(false);
            inGame = false;
        }
    }

    void Endless() {
        if (TanksLeft() < tanks.Length) {
            for (int i = 0; i < tanks.Length; i++) {
                GameObject tank;
                if (!tanks[i].activeSelf && !tanks[i].CompareTag("Player")) {
                    tanksDestroyed += 1;

                    tank = tanks[i];
                    tank.SetActive(true);

                    tank.GetComponent<EnemyMovement>().enabled = false;
                    tank.GetComponent<EnemyShooting>().enabled = false;

                    tank.GetComponent<EnemyMovement>().enabled = true;
                    tank.GetComponent<EnemyShooting>().enabled = true;
                }
            }
        }

        timerText.text = tanksDestroyed.ToString();
        
        if (IsPlayerDead() || inGame == false) {
            messageText.gameObject.SetActive(true);

            player.clip = pregameMusic;
            player.Play();

            if (inGame == false) {
                timerText.text = "";
                totalTime.text = "";
                messageText.text = "Tankinator.";
            }
            else if (IsPlayerDead()) {
                messageText.text = "Tankinator-ed.";
            }

            endlessHighscores.AddScore(tanksDestroyed);
            endlessHighscores.SaveScoresToFile();

            playButton.gameObject.SetActive(true);
            endlessButton.gameObject.SetActive(true);
            highscoresButton.gameObject.SetActive(true);
            TankSelectionButton.gameObject.SetActive(true);

            SetTanksActive(false);
            inGame = false;
            tanksDestroyed = 0;
        }
    }

    void SetTanksActive(bool active)
    {
        for (int i = 0; i < tanks.Length; i++)
        {
            var tank = tanks[i];
            var materialAccess = tank.GetComponent<MaterialAccess>();

            bool isPlayer = (i == selectedTankIndex);

            foreach (MeshRenderer rend in materialAccess.meshes)
            {

                if (isPlayer)
                {
                    tank.tag = "Player";
                    rend.material = active ? playerMat : neutralMat;

                    tank.GetComponent<TankMovement>().enabled = active;
                    tank.GetComponent<TankAim>().enabled = active;
                    tank.GetComponent<TankShooting>().enabled = active;
                }
                else
                {
                    tank.tag = "Enemy";
                    rend.material = active ? enemyMat : neutralMat;

                    tank.GetComponent<EnemyMovement>().enabled = active;
                    tank.GetComponent<EnemyShooting>().enabled = active;
                }

                tank.GetComponent<TankHealth>().enabled = active;
            }
        }
    }

    int TanksLeft() {
        int tankCount = 0;

        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].activeSelf) tankCount++;
        }

        return tankCount;
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

    public void OnTankSelection()
    {
        TankSelectionPanel.SetActive(true);
        TankSelectionButton.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        endlessButton.gameObject.SetActive(false);
        highscoresButton.gameObject.SetActive(false);
    }

    void CloseTankSeletion()
    {
        TankSelectionPanel.SetActive(false);
        TankSelectionButton.gameObject.SetActive(true);
        playButton.gameObject.SetActive(true);
        endlessButton.gameObject.SetActive(true);
        highscoresButton.gameObject.SetActive(true);
    }

    int selectedTankIndex = 0;
    public void SelectLightTank()
    {
        selectedTankIndex = 1;
        LighTank.tag = "player";
        CloseTankSeletion();
    }
    public void SelectMainTank() {
        selectedTankIndex = 2;
        MeduimTank.tag = "player";
        CloseTankSeletion();
    }
    public void SelectHeavyTank() {
        selectedTankIndex = 3;
        HeavyTank.tag = "player";
        CloseTankSeletion();
    }
}