using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Diagnostics;

public class GameManager : MonoBehaviour
{
    #region 스테이지 관리 변수
    public int stage;

    public Animator LevelAnim;
    public Animator StartAnim;
    public Animator ClearAnim;

    public Animator FadeAnim;

    public Transform PlayerPos;
    #endregion

    public string[] enemyObjs;
    public Transform[] spawnPoints;

    public float nextSpawnDelay;
    public float curSpawnDelay;

    public GameObject player;
    public Text scoreText;
    public Image[] lifeImage;
    public Image[] boomImage;

    public GameObject gameOverSet;
    public ObjectManager objectManager;


    public List<Spawn> spawnList;
    public int spawnIndex;
    public bool spawnEnd;


    void Awake()
    {
        spawnList = new List<Spawn>();
        enemyObjs = new string[] { "EnemyS", "EnemyM", "EnemyL", "EnemyB" };
        StartToLevel();
    }

    #region 스테이지 관리
    public void StartToLevel()
    {
        // Level, Start ui
        LevelAnim.SetTrigger("On");
        LevelAnim.GetComponent<Text>().text = "Level " + stage;
        StartAnim.SetTrigger("On");

        // 적 스폰 파일
        ReadSpawnFile();

        // Fade In
        FadeAnim.SetTrigger("In");
    }

    public void ClearToLevel()
    {
        // Clear ui
        ClearAnim.SetTrigger("On");

        // Fade Out
        FadeAnim.SetTrigger("Out");

        // Player 위치 재설정
        player.transform.position = PlayerPos.position;

        // Level 증가
        stage++;
        if (stage > 3)
            Invoke("GameOver", 6);
        else
            Invoke("StartToLevel", 5);
        
    }
    #endregion


    void ReadSpawnFile()
    {
        //.#1.변수초기화
        spawnList.Clear();
        spawnIndex = 0;
        spawnEnd = false;

        // #2.리스폰 파일 읽기
        TextAsset textFile = Resources.Load("Stage" + stage) as TextAsset;
        StringReader stringReader = new StringReader(textFile.text);

        //#3.한 줄씩 데이터 저장
        while(stringReader != null)
        {
            string line = stringReader.ReadLine();
          
            if (line == null)
                break;

            //#.리스폰 데이터 생성
            Spawn spawnData = new Spawn();
            spawnData.delay = float.Parse(line.Split(',')[0]);
            spawnData.type = line.Split(',')[1];
            spawnData.point = int.Parse(line.Split(',')[2]);
            spawnList.Add(spawnData);
        }

        //#.텍스트 파일 닫기
        stringReader.Close();

        //#.첫번째 스폰 딜레이 적용
         nextSpawnDelay = spawnList[0].delay;
        
    }

    //적 생성 딜레이
    void Update()
    {
        curSpawnDelay += Time.deltaTime;

        if(curSpawnDelay > nextSpawnDelay && !spawnEnd)
        {
            SpawnEnemy();
            nextSpawnDelay = Random.Range(0.5f, 3f);
            curSpawnDelay = 0;
        }

        //#.UI Score Update
        Player playerLogic = player.GetComponent<Player>();
        scoreText.text = string.Format("{0:n0}", playerLogic.score);
    }

    //랜덤 적(L,M,S)&위치에 Enemy 생성
    void SpawnEnemy()
    {
        int enemyIndex = 0;
        switch (spawnList[spawnIndex].type)
        {
            case "S":
                enemyIndex = 0;
                break;
            case "M":
                enemyIndex = 1;
                break;
            case "L":
                enemyIndex = 2;
                break;
            case "B":
                enemyIndex = 3;
                break;
        }
        int enemyPoint = spawnList[spawnIndex].point;
        GameObject enemy = objectManager.MakeObj(enemyObjs[enemyIndex]);
        enemy.transform.position = spawnPoints[enemyPoint].position;

        Rigidbody2D rigid = enemy.GetComponent<Rigidbody2D>();
        Enemy enemyLogic = enemy.GetComponent<Enemy>();
        enemyLogic.gameManager = this;
        enemyLogic.player = player;
        enemyLogic.objectManager = objectManager;

        if (enemyPoint == 5 || enemyPoint == 6) // Right spawn
        {
            enemy.transform.Rotate(Vector3.back * 90);
            rigid.velocity = new Vector2(enemyLogic.speed * (-1), 1);
        }
        else if (enemyPoint == 7 || enemyPoint == 8) // Left spawn
        {
            enemy.transform.Rotate(Vector3.forward * 90);
            rigid.velocity = new Vector2(enemyLogic.speed, -1);
        }
        else // Front spwan
        {
            rigid.velocity = new Vector2(0, enemyLogic.speed * (-1));
        }

        //#.리스폰 인덱스 증가
        spawnIndex++;
        if(spawnIndex == spawnList.Count)
        {
            spawnEnd = true;
            return;
        }

        //#.다음 리스폰 딜레이 갱신
        nextSpawnDelay = spawnList[spawnIndex].delay;
    }

    public void UpdateLifeIcon(int life)
    {
        //#.UI Life Init Disable
        for (int index = 0; index < 3; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 0);
        }

        //#.UI Life Active
        for (int index = 0; index < life; index++)
        {
            lifeImage[index].color = new Color(1, 1, 1, 1);
        }
    }

    public void UpdateBoomIcon(int boom)
    {
        //#.UI boom Init Disable
        for (int index = 0; index < 3; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 0);
        }

        //#.UI boom Active
        for (int index = 0; index < boom; index++)
        {
            boomImage[index].color = new Color(1, 1, 1, 1);
        }
    }


    public void RespawnPlayer()
    {
        Invoke("RespawnPlayerExe", 2f);
    }

    void RespawnPlayerExe()
    {
        player.transform.position = Vector3.down * 3.5f;
        player.SetActive(true);

        Player playerLogic = player.GetComponent<Player>();
        playerLogic.isHit = false;
    }

    public void GameOver()
    {
        gameOverSet.SetActive(true);
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }
}
