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

    public Transform destructableScenery;
    public GameObject[] tanks;
    public GameObject tanksParent;

    float gameTime = 0f;
    public float GameTime { get {return gameTime;} }
    int tanksDestroyed = 0;
    int currentTanks = 2;

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

        timerText.text = "";
        totalTime.text = "";
        messageText.text = "Tankinator.";

        highscorePanel.SetActive(false);
        playButton.gameObject.SetActive(true);
        endlessButton.gameObject.SetActive(true);
        highscoresButton.gameObject.SetActive(true);

        // SetTanksActive(tanks.Length, false, false);
    }

    void Update() {
        // Returns to the title screen on ESC or quits if alreadyy at the title screen.
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

        // Transfers game states.
        if (inGame) {
            if (endless) Endless();
            else Ingame();
        }
        else Pregame();
    }

    // The highscore button activates and disables the highscore menu.
    public void OnHighscores() {
        messageText.gameObject.SetActive(!messageText.gameObject.activeSelf);
        highscorePanel.SetActive(!highscorePanel.gameObject.activeSelf);

        ViewHighscores();
    }

    // displays endless highscores or regular highscores.
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

    // Regular and Endless Buttons use this. It plays the game,
    // or when in highscores view, changes which scores are viewed.
    public void Play(bool endless) {
        this.endless = endless;

        if (!highscorePanel.activeSelf) keyDown = true;
        else ViewHighscores();
    }

    void Pregame() {
        // press any key except for mouse buttons function
        // if (Input.anyKeyDown) {
        //     if (Input.GetMouseButton(0) ||
        //         Input.GetMouseButton(1) ||
        //         Input.GetMouseButton(2)) return;
            
        //     keyDown = true;
        // }
        
        // plays the game when {keyDown} is true, usually via a button.
        if (keyDown) {
            keyDown = false;

            highscorePanel.SetActive(false);
            playButton.gameObject.SetActive(false);
            endlessButton.gameObject.SetActive(false);
            highscoresButton.gameObject.SetActive(false);

            messageText.gameObject.SetActive(false);

            totalTime.text = "";

            if (endless) {
                for (int i = 0; i < tanks.Length; i++) {
                    tanks[i].SetActive(false);
                }

                SetTanksActive(currentTanks, true, false);
            }
            else SetTanksActive(tanks.Length, true, false);

            gameTime = 0;
            inGame = true;

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

        // When the game ends, enter the post-game ui and reset.
        if (IsPlayerDead() || TanksLeft() <= 1 || inGame == false) {
            messageText.gameObject.SetActive(true);

            player.clip = pregameMusic;
            player.Play();

            if (inGame == false) {
                // Executed if the player quits.
                timerText.text = "";
                totalTime.text = "";
                messageText.text = "Tankinator.";
            }
            else if (IsPlayerDead()) {
                // Executed if the player is dead.
                messageText.text = "Tankinator-ed.";
            }
            else {
                if (IsPlayerScratchless()) {
                    // Executed if the player won without being damaged.
                    // This will also decrease the seconds by how many enemy tanks there were.

                    messageText.text = "Flawless Tankinator.";

                    timerText.text += $" - {tanks.Length - 1}";
                    seconds -= tanks.Length - 1;
                }
                else {
                    // Executed if the player won but was damaged.
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

            SetTanksActive(tanks.Length, false, false);
            inGame = false;
        }
    }

    void Endless() {
        timerText.text = tanksDestroyed.ToString();

        // When the game ends, enter the post-game ui and reset.
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

            SetTanksActive(tanks.Length, false, false);
            inGame = false;
            tanksDestroyed = 0;
        }

        // when a tank is destroyed, create up to 2 more.
        if (TanksLeft() < currentTanks) {
            tanksDestroyed++;
            if (currentTanks < tanks.Length) currentTanks++;

            SetTanksActive(currentTanks - TanksLeft(), true, true);
        }
    }

    // runs SetTankActive() for the amount of tanks specified by {count}.

    /// <summary>
    /// if skipActiveTanks = true, then it will skip past a tank if it is active,
    /// and move onto another. This is used when wanting to activate a certain
    /// amount of tanks at one time.
    /// </summary>
    void SetTanksActive(int count, bool active, bool skipActiveTanks)
    {
        count = Mathf.Min(count, tanks.Length);

        for (int i = 0; i < count; i++) {
            if (tanks[i].activeSelf && skipActiveTanks) {
                count++;

                if (count > tanks.Length) {
                    Debug.Log("Maximum Tank Capacity Reached.");
                    count = tanks.Length;
                }

                continue;
            }

            SetTankActive(i, active);
        }
    }

    void SetTankActive(int index, bool active)
    {
        // gets the selected tank with {index}
        var tank = tanks[index];
        if (!tank) {
            Debug.LogWarning($"Tank of index {index} does not exist.");
            return;
        }

        // gets access to the tank's MaterialAccess script
        var materialAccess = tank.GetComponent<MaterialAccess>();
        if (!materialAccess || materialAccess.meshes == null) {
            Debug.LogWarning($"Tank of index {index} does not have 'MaterialAccess'.");
            return;
        }

        if (active) tank.SetActive(true);

        // enables/disables all of the tank's essential scripts, and their colour.
        tank.GetComponent<TankHealth>().enabled = active;

        if (tank.CompareTag("Player")) {
            tank.GetComponent<TankMovement>().enabled = active;
            tank.GetComponent<TankAim>().enabled = active;
            tank.GetComponent<TankShooting>().enabled = active;

            foreach (MeshRenderer rend in materialAccess.meshes) {
                rend.material = active ? playerMat : neutralMat;
            }
        }
        else {
            tank.GetComponent<EnemyMovement>().enabled = active;
            tank.GetComponent<EnemyShooting>().enabled = active;

            foreach (MeshRenderer rend in materialAccess.meshes) {
                rend.material = active ? enemyMat : neutralMat;
            }
        }
    }

    // Checks how man tanks are active and returns the count.
    int TanksLeft() {
        int tankCount = 0;

        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].activeSelf) tankCount++;
        }

        return tankCount;
    }

    // Checks if the player is dead.
    bool IsPlayerDead() {
        for (int i = 0; i < tanks.Length; i++) {
            if (!tanks[i].activeSelf && tanks[i].CompareTag("Player")) return true;
        }

        return false;
    }

    // Checks if the player has taken any damage.
    bool IsPlayerScratchless() {
        for (int i = 0; i < tanks.Length; i++) {
            if (tanks[i].GetComponent<TankHealth>().scratchless
                && tanks[i].CompareTag("Player")) return true;
        }

        return false;
    }
}