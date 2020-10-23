using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Specialized;
using System.Diagnostics;


public class DropObjectsManager : MonoBehaviour
{
    public Camera MainCamera;
    private List<GameObject> _dropinventory;
    private int _policysize = 30;
    public Boolean ctrlpressed = false;
    public Vector3 cameraposition;
    public TextMesh lasthover;
    public SimulContext simc;

    // Start is called before the first frame update
    void Awake()
    {
        _dropinventory = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ctrlpressed == true)
        {
            HoverObject();
            if (Input.GetMouseButtonDown(0))
            {
                DropObject();
            }

        }
    }

    public void CreateNewObject(string name)
    {
        UnityEngine.Debug.Log("Creation objet vide");
        GameObject newobj = new GameObject(name+".drop");
        newobj.SetActive(false);
        newobj.AddComponent(typeof(TextMesh));
        newobj.AddComponent(typeof(BoxCollider));
        newobj.GetComponent<TextMesh>().fontSize = _policysize;
        newobj.GetComponent<TextMesh>().fontStyle = FontStyle.Bold;
        newobj.GetComponent<TextMesh>().text = name;
        TextMesh mesh = newobj.GetComponent<TextMesh>();
        BoxCollider box = newobj.GetComponent<BoxCollider>();
        box.size = new Vector3(GetWidth(mesh), _policysize*0.1f, 1);
        float x = (float)(box.center[0] + GetWidth(mesh) * 0.5);
        float y = (float)(box.center[1] - _policysize * 0.1f * 0.5);
        box.center = new Vector3(x, y, box.center[2]);
        _dropinventory.Add(newobj);
    }

    public void HoverObject()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100000))
        {
            string name = hit.collider.gameObject.name;
            UnityEngine.Debug.Log("SELECTION MANAGER : object detected (" + name + ")");
            if(_dropinventory.Contains(GameObject.Find(name)))
            {
                GameObject obj = GameObject.Find(name);
                obj.GetComponent<TextMesh>().color = Color.blue;
                lasthover = obj.GetComponent<TextMesh>();
            }
        }
        else if(lasthover != null)
        {
            lasthover.color = Color.white;
            lasthover = null;
        }
    }

    public void DropObject()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        UnityEngine.Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100000))
        {
            string name = hit.collider.gameObject.name;
            if (_dropinventory.Contains(GameObject.Find(name)))
            {
                GameObject obj = GameObject.Find(name);
                obj.SetActive(false);
                _dropinventory.Remove(obj);
                UnityEngine.Debug.Log("nom 1 : " + name);
                name = name.Split('.')[0];
                UnityEngine.Debug.Log("nom 2 : " +name);
                simc.DropObj(name);
            }
        }
        else UnityEngine.Debug.Log("SELECTION MANAGER : nothing selected");
    }

    public void PrintDropList()
    {
        UnityEngine.Debug.Log("Print !!!!");
        int pas = 0;
        Vector3 cameraposbis,sp, v;
        float x = cameraposition[0];
        float y = cameraposition[1];
        float z = cameraposition[2];
        foreach (GameObject obj in _dropinventory)
        {
            cameraposbis = new Vector3(x,y,z);
            UnityEngine.Debug.Log(cameraposbis);
            sp = MainCamera.ViewportToScreenPoint(cameraposbis);
            v = MainCamera.ScreenToWorldPoint(new Vector3(sp[0]-50, sp[1] + 200 - pas, sp[2] + 50));
            obj.transform.position= v;
            obj.SetActive(true);
            pas = pas + _policysize + 15;
        }
    }

    public float GetWidth(TextMesh mesh)
    {
        float width = 0;
        foreach (char symbol in mesh.text)
        {
            CharacterInfo info;
            if (mesh.font.GetCharacterInfo(symbol, out info, mesh.fontSize, mesh.fontStyle))
            {
                width += info.advance;
            }
        }
        return width * mesh.characterSize * 0.1f;
    }

    public void UnPrint()
    {
        foreach(GameObject obj in _dropinventory)
        {
            obj.SetActive(false);
        }
    }
}
