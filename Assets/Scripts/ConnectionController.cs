using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json;

public partial class ConnectionController : MonoBehaviour
{
    public struct Connection
    {
        private string _id;

        public Guid from;
        public Guid to;

        public ConnectionType type;

        public bool IsValid() => from != Guid.Empty && to != Guid.Empty;

        public override bool Equals(object obj)
        {
            return obj is Connection connection && this == connection;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(from, to, type);
        }

        public static implicit operator bool(Connection connection) => connection.IsValid();

        public static bool operator ==(Connection left, Connection right)
        {
            return left.from == right.from && left.to == right.to && left.type == right.type;
        }
        public static bool operator !=(Connection left, Connection right)
        {
            return !(left == right);
        }
    }

    public static ConnectionController Instance;

    public bool isMakingConnection => _isPreparingConnection;

    [SerializeField] Arrow _arrowPrefab;
    [SerializeField] Arrow _marriageLinePrefab;
    [SerializeField] Arrow _parentageLinePrefab;

    private List<Connection> _connections = new List<Connection>();
    private Dictionary<Connection, Arrow> _arrows = new Dictionary<Connection, Arrow>();

    private bool _isPreparingConnection = false;
    private Guid _fromGuid;
    private Arrow _preparingArrow;
    private ConnectionType _preparingConnectionType;
    private bool _dontDestroy = false;
    private Camera _camera;

    private void Start()
    {
        Instance = this;
        _camera = Camera.main;
    }

    public void PrepareConnection(Guid fromGuid, ConnectionType type)
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

    public void MakeConnection(Guid toGuid)
    {
        if (_isPreparingConnection)
        {
            var connections = _connections.Where(x =>
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
                if (connection)
                {
                    _connections.Remove(connection);
                    Destroy(_arrows[connection].gameObject);
                    _arrows.Remove(connection);
                    return;
                }
            }

            if (_fromGuid.Equals(toGuid)) return;

            _connections.Add(new Connection
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

            _preparingArrow.Set(element.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition), 0, 0);

            if (Input.GetMouseButtonUp(0) && !_dontDestroy)
            {
                Destroy(_preparingArrow.gameObject);
            }
        }

        foreach (var connection in _connections)
        {
            var element1 = GridViewport.Instance.GetElement(connection.from);
            var element2 = GridViewport.Instance.GetElement(connection.to);

            if (element1 && element2)
            {
                if (element1.IsOutOfScreenBounds() && element2.IsOutOfScreenBounds()) continue;

                if (!_arrows.ContainsKey(connection))
                {
                    switch (connection.type)
                    {
                        case ConnectionType.OneWay:
                            _arrows[connection] = GridViewport.Instance.InstantiateChild(_arrowPrefab).GetComponent<Arrow>();
                            break;

                        case ConnectionType.Marriage:
                            _arrows[connection] = GridViewport.Instance.InstantiateChild(_marriageLinePrefab).GetComponent<Arrow>();
                            break;

                        case ConnectionType.Parentage:
                            _arrows[connection] = GridViewport.Instance.InstantiateChild(_parentageLinePrefab).GetComponent<Arrow>();
                            break;

                    }
                    GridViewport.Instance.SetBehind(_arrows[connection].transform);
                }

                Vector2 direction = element2.Transform.position - element1.Transform.position;
                float angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
                _arrows[connection].Set(element1.Transform.position, element2.Transform.position, element1.GetConnectionOffset(angle, connection.type), element2.GetConnectionOffset(angle, connection.type));
            }
            else
            {
                if (_arrows.TryGetValue(connection, out var arrow))
                {
                    Destroy(arrow.gameObject);
                    _arrows.Remove(connection);
                }
                _connections.Remove(connection);
                break;
            }
        }

        _dontDestroy = false;
    }

    public virtual string Serialize()
    {
        List<SerializableConnection> serializables = new List<SerializableConnection>();
        for (int i = 0; i < _connections.Count; i++) serializables.Add(_connections[i]);
        return JsonConvert.SerializeObject(serializables);
    }

    public virtual void Deserialize(string str)
    {
        Clear();
        List<SerializableConnection> serializables = JsonConvert.DeserializeObject<List<SerializableConnection>>(str);
        for (int i = 0; i < serializables.Count; i++) _connections.Add(serializables[i]);
    }

    public void Clear()
    {
        foreach (var a in _arrows)
        {
            Destroy(a.Value.gameObject);
        }
        _connections.Clear();
        _arrows.Clear();
    }
}
