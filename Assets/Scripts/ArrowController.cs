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
        public ConnectionType type = ConnectionType.OneWay;

        public string id => from + to;
    }

    public static ArrowController Instance;

    public bool isMakingConnection => _isPreparingConnection;

    [SerializeField] Arrow _arrowPrefab;
    [SerializeField] Arrow _marriageLinePrefab;
    [SerializeField] Arrow _parentageLinePrefab;

    private List<Connection> _arrowConnections = new List<Connection>();
    private Dictionary<string, Arrow> _arrows = new Dictionary<string, Arrow>();

    private bool _isPreparingConnection = false;
    private string _fromGuid = "";
    private Arrow _preparingArrow;
    private ConnectionType _preparingConnectionType;
    private bool _dontDestroy = false;

    private void Start()
    {
        Instance = this;
    }

    public void PrepareConnection(string fromGuid, ConnectionType type)
    {
        _isPreparingConnection = true;
        _fromGuid = fromGuid;
        _preparingConnectionType = type;

        switch (type)
        {
            case ConnectionType.OneWay:
                _preparingArrow = GridViewport.Instance.InstantiateChild(_arrowPrefab.gameObject).GetComponent<Arrow>();
                break;

            case ConnectionType.Marriage:
                _preparingArrow = GridViewport.Instance.InstantiateChild(_marriageLinePrefab.gameObject).GetComponent<Arrow>();
                break;

            case ConnectionType.Parentage:
                _preparingArrow = GridViewport.Instance.InstantiateChild(_parentageLinePrefab.gameObject).GetComponent<Arrow>();
                break;
        }

        _dontDestroy = true;
    }

    public void MakeConnection(string toGuid)
    {
        if (_isPreparingConnection)
        {
            var connections = _arrowConnections.Where(x =>
            {
                if (x.type == _preparingConnectionType)
                {
                    if (x.from.Equals(_fromGuid) && x.to.Equals(toGuid))
                    {
                        return true;
                    }
                    else if (x.from.Equals(toGuid) && x.to.Equals(_fromGuid))
                    {
                        return true;
                    }
                }
                return false;
            });

            foreach (var connection in connections)
            {
                if (connection != null)
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
                to = toGuid,
                type = _preparingConnectionType
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
            //CameraController.Instance.disable = true;

            var element = GridViewport.Instance.GetElement(_fromGuid);

            _preparingArrow.Set(element.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), 0);

            if (Input.GetMouseButtonUp(0) && !_dontDestroy)
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
                switch (connection.type)
                {
                    case ConnectionType.OneWay:
                        _arrows[connection.id] = GridViewport.Instance.InstantiateChild(_arrowPrefab).GetComponent<Arrow>();
                        break;

                    case ConnectionType.Marriage:
                        _arrows[connection.id] = GridViewport.Instance.InstantiateChild(_marriageLinePrefab).GetComponent<Arrow>();
                        break;

                    case ConnectionType.Parentage:
                        _arrows[connection.id] = GridViewport.Instance.InstantiateChild(_parentageLinePrefab).GetComponent<Arrow>();
                        break;

                }
                GridViewport.Instance.SetBehind(_arrows[connection.id].transform);
            }

            _arrows[connection.id].Set(element1.transform.position, element2.transform.position, connection.type == ConnectionType.OneWay ? 100 : 0);
        }

        _dontDestroy = false;
    }

    public virtual string Serialize()
    {
        return JsonConvert.SerializeObject(_arrowConnections);
    }

    public virtual void Deserialize(string str)
    {
        Clear();
        _arrowConnections = JsonConvert.DeserializeObject<List<Connection>>(str);
    }

    public void Clear()
    {
        foreach (var a in _arrows)
        {
            Destroy(a.Value.gameObject);
        }
        _arrowConnections.Clear();
        _arrows.Clear();
    }
}
