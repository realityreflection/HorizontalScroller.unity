using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScroll : MonoBehaviour
{
    public float selectedScale = 1.0f;
    public float normalScale = 0.6f;

    public int elementWidth = 250;

    public int idx { get; private set; }

    private Vector3 prevPosition;
    private GameObject[] contents;

    private float idxf = 0;
    private float x = 0;

    void Awake()
    {
        var list = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var rt = transform.GetChild(i).GetComponent<RectTransform>();
            list.Add(transform.GetChild(i).gameObject);

            rt.anchoredPosition = new Vector2(i * elementWidth, 0);

            if (idx != i)
                rt.transform.localScale = Vector3.one * normalScale;
            else
                rt.transform.localScale = Vector3.one * selectedScale;
        }

        contents = list.ToArray();
    }

	public void OnTouchDown()
    {
        prevPosition = Input.mousePosition;

        StopAllCoroutines();
        StartCoroutine(ScrollFunc());
    }
    public void OnTouchUp()
    {
        StopAllCoroutines();
        StartCoroutine(AdjustFunc());
    }

    /// <summary>
    /// 손을 이동중일때의 스크롤링
    /// </summary>
    IEnumerator ScrollFunc()
    {
        while (true)
        {
            yield return null;

            var delta = Input.mousePosition - prevPosition;

            for (int i = 0; i < transform.childCount; i++)
            {
                var rt = transform.GetChild(i).GetComponent<RectTransform>();

                rt.anchoredPosition += new Vector2(delta.x, 0);
                rt.localScale = CalcScale(i);
            }
            x = Mathf.Clamp(x + delta.x, -elementWidth * contents.Length, 0);

            idxf = -((float)x) / elementWidth;
            idx = (int)idxf;

            prevPosition = Input.mousePosition;
        }
    }
    /// <summary>
    /// 손을 놓았을 때, 딱 맞는 위치로 이동시킨다.
    /// </summary>
    IEnumerator AdjustFunc()
    {
        var targetScales = new Vector3[contents.Length];
        var targetPositions = new Vector2[contents.Length];

        if (idxf - idx >= 0.5f)
            idx++;
        else if (idxf - idx <= -0.5f)
            idx--;
        idx = Mathf.Clamp(idx, 0, contents.Length-1);

        var diff = (contents[idx].GetComponent<RectTransform>().anchoredPosition.x);
        idxf = idx;
        x = idx * -elementWidth;

        for (int i = 0; i < transform.childCount; i++)
        {
            var rt = transform.GetChild(i).GetComponent<RectTransform>();

            targetScales[i] = CalcScale(i);
            targetPositions[i] = rt.anchoredPosition - new Vector2(diff, 0);
        }

        for (int f = 0; f < 30; f++)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var rt = transform.GetChild(i).GetComponent<RectTransform>();

                rt.anchoredPosition += (targetPositions[i] - rt.anchoredPosition) * 0.15f;
                rt.localScale += (targetScales[i] - rt.localScale) * 0.15f;
            }

            yield return null;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var rt = transform.GetChild(i).GetComponent<RectTransform>();

            rt.anchoredPosition = targetPositions[i];
            rt.localScale = targetScales[i];
        }
    }


    /// <summary>
    /// i번째 아이템의 스케일을 결정한다.
    /// </summary>
    /// <param name="i"></param>
    private Vector3 CalcScale(int i)
    {
        if (idx == i)
            return (Vector3.one * selectedScale) - (Vector3.one * (selectedScale - normalScale) * (idxf - idx));
        else if ((idxf >= idx && idx == i - 1) ||
                 (idxf < idx) && idx == i + 1)
            return Vector3.one * normalScale + (Vector3.one * (selectedScale - normalScale) * (idxf - idx));
        else
            return Vector3.one * normalScale;
    }
}
