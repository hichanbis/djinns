using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AnyButtonSelect : MonoBehaviour, ISelectHandler
{
    private GameObject cursor;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cursor = GetComponentInParent<BattleUI>().cursor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(LerpCursorToMe());
    }

    public IEnumerator LerpCursorToMe()
    {
        yield return null;

        float elapsedTime = 0f;
        float transformWidth = RectTransformToScreenSpace(rectTransform).width;
        float cursorWidth = RectTransformToScreenSpace(cursor.GetComponent<RectTransform>()).width;
        Vector3 dest = new Vector3(transform.position.x - (transformWidth / 2) - (cursorWidth / 2), transform.position.y, transform.position.z);

        while (elapsedTime < 0.1f)
        {
            cursor.transform.position = Vector3.Lerp(cursor.transform.position, dest, 25f * Time.deltaTime); 
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    public Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }
}
