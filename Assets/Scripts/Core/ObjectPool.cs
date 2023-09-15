using System;
using System.Collections.Generic;
using System.Linq;

public class ObjectPool<T>
{
    public delegate T FactoryMethod();

    private FactoryMethod _factoryMethod;
    private Queue<T> _currentStock;
    private bool _isDynamic;

    private Action<T> _turnOnCallback;
    private Action<T> _turnOffCallback;

    public ObjectPool(FactoryMethod factoryMethod, Action<T> turnOnCallback, Action<T> turnOffCallback, int initialStock = 0, bool isDynamic = true)
    {
        _factoryMethod = factoryMethod;
        _turnOnCallback = turnOnCallback;
        _turnOffCallback = turnOffCallback;

        _isDynamic = isDynamic;

        _currentStock = new Queue<T>();

        for (int i = 0; i < initialStock; i++)
        {
            var obj = _factoryMethod();
            _turnOffCallback(obj);
            _currentStock.Enqueue(obj);
        }
    }

    public T GetObject()
    {
        var result = default(T);

        if (_currentStock.Any())
        {
            result = _currentStock.Dequeue();
        }
        else if (_isDynamic)
        {
            result = _factoryMethod();
        }

        _turnOnCallback(result);

        return result;
    }

    public void ReturnObject(T obj)
    {
        _turnOffCallback(obj);
        _currentStock.Enqueue(obj);
    }
}
