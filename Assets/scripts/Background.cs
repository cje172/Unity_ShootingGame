using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region 배경 개발(스크롤링, 패럴렉스)
public class Background : MonoBehaviour
{
    public float speed;

    public int sIndex;
    public int eIndex;
    public Transform[] reBackground;

    float CameraHeight;

    private void Awake()
    {
        CameraHeight = Camera.main.orthographicSize * 2;
    }

    void Update()
    {
        BackMove();
        BackRepeat();
    }

    // 배경 이동(움직임)
    void BackMove()
    {
        Vector3 cPos = transform.position;
        Vector3 nPos = Vector3.down * speed * Time.deltaTime;
        transform.position = cPos + nPos;
    }

    // 배경 재사용
    void BackRepeat()
    {
        if (reBackground[eIndex].position.y < CameraHeight * (-1) * 2)
        {
            Vector3 bPos = reBackground[eIndex].localPosition;
            Vector3 fPos = reBackground[sIndex].localPosition;
            reBackground[eIndex].transform.localPosition = fPos + Vector3.up * 24;

            int temp = sIndex;
            sIndex = eIndex;
            eIndex = (temp - 1) == -1 ? reBackground.Length - 1 : (temp - 1);
        }
    }
}
#endregion
