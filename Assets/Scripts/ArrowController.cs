using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;

public class ArrowController : MonoBehaviour
{
    [Serializable]
    public class Connection
    {
        public string from;
        public string to;

        public string id => from + to;
    }

    public static ArrowController Instance;

    public bool isMakingConnection => _isPreparingConnection;

    [SerializeField] Arrow _arrowPrefab;

    private List<Connection> _arrowConnections = new List<Connection>();
    private Dictionary<string, Arrow> _arrows = new Dictionary<string, Arrow>();

    private bool _isPreparingConnection = false;
    private string _fromGuid = "";
    private Arrow _preparingArrow;
    private bool _dontDestroy = false;

    private void Start()
    {
        Instance = this;
    }

    public void PrepareConnection(string fromGuid)
    {
        _isPreparingConnection = true;
        _fromGuid = fromGuid;

        _preparingArrow = GridViewport.Instance.InstantiateChild(_arrowPrefab.gameObject).GetComponent<Arrow>();
        _dontDestroy = true;
    }

    public void MakeConnection(string toGuid)
    {
        if (_isPreparingConnection)
        {
            var connections = _arrowConnections.Where(x => x.from.Equals(_fromGuid));

            foreach (var connection in connections)
            {
                if (connection != null && connection.to.Equals(toGuid))
                {
                    _arrowConnections.Remove(connection);
                    Destroy(_arrows[connection.id].gameObject);
                    _arrows.Remove(connection.id);
                    return;
                }
            }

            if (_fromGuid.Equals(toGuid)) return;

            _arrowConnections.Add(new Connection
            {
                from = _fromGuid,
                to = toGuid
            });
        }
    }

    private void LateUpdate()
    {
        if (!_preparingArrow)
        {
            _isPreparingConnection = false;
        }
        if (_isPreparingConnection)
        {
            CameraController.Instance.disable = true;

            var element = GridViewport.Instance.GetElement(_fromGuid);

            _preparingArrow.Set(element.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), 0);

            if (Input.GetMouseButtonUp(0) && !_dontDestroy)
            {
                Destroy(_preparingArrow.gameObject);
            }
            if (Input.GetMouseButtonUp(1) && !_dontDestroy)
            {
                Destroy(_preparingArrow.gameObject);
            }
        }

        foreach(var connection in _arrowConnections)
        {
            var element1 = GridViewport.Instance.GetElement(connection.from);
            var element2 = GridViewport.Instance.GetElement(connection.to);

            if(!_arrows.ContainsKey(connection.id))
            {
                _arrows[connection.id] = GridViewport.Instance.InstantiateChild(_arrowPrefab).GetComponent<Arrow>();
                GridViewport.Instance.SetBehind(_arrows[connection.id].transform);
            }

            _arrows[connection.id].Set(element1.transform.position, element2.transform.position, 100);
        }

        _dontDestroy = false;
    }

    public virtual string Serialize()
    {
        return JsonConvert.SerializeObject(_arrowConnections);
    }

    public virtual void Deserialize(string str)
    {
        foreach(var a in _arrows)
        {
            Destroy(a.Value.gameObject);
        }
        _arrows.Clear();
        _arrowConnections = JsonConvert.DeserializeObject<List<Connection>>(str);
    }
}
