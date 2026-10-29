using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour
{
    [SerializeField] List<GameObject> _listIcon;

    int _curIteration;

    void Start()
    {
        _curIteration = 0;

        foreach (GameObject o in _listIcon)
        {
            o.SetActive(false);
        }
    }


    public Image AddIcon(Sprite i)
    {
        Image img = _listIcon[_curIteration].GetComponent<Image>();
        img.sprite = i;

        _listIcon[_curIteration].SetActive(true);

        _curIteration++;
        return img;
    }

}
