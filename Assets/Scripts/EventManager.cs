using System.Collections.Generic;
using System;

public class GameEvent { }

public class EventManager
{
	public delegate void EventDelegate<T>(T e) where T : GameEvent;
	private delegate void EventDelegate(GameEvent e);

	private readonly Dictionary<Type, EventDelegate> _delegates = new Dictionary<Type, EventDelegate>();
	private readonly Dictionary<Delegate, EventDelegate> _delegateLookup = new Dictionary<Delegate, EventDelegate>();

	private static readonly EventManager _instance = new EventManager();
	public static EventManager Instance => _instance;

	private EventManager() { }

	public void AddHandler<T>(EventDelegate<T> del) where T : GameEvent
	{
		if (_delegateLookup.ContainsKey(del)) return;

		EventDelegate internalDelegate = (e) => del((T)e);
		_delegateLookup[del] = internalDelegate;

		EventDelegate tempDel;
		if (_delegates.TryGetValue(typeof(T), out tempDel))
		{
			_delegates[typeof(T)] = tempDel += internalDelegate;
		}
		else
		{
			_delegates[typeof(T)] = internalDelegate;
		}
	}

	public void RemoveHandler<T>(EventDelegate<T> del) where T : GameEvent
	{
		EventDelegate internalDelegate;
		if (_delegateLookup.TryGetValue(del, out internalDelegate))
		{
			EventDelegate tempDel;
			if (_delegates.TryGetValue(typeof(T), out tempDel))
			{
				tempDel -= internalDelegate;
				if (tempDel == null)
				{
					_delegates.Remove(typeof(T));
				}
				else
				{
					_delegates[typeof(T)] = tempDel;
				}
			}
			_delegateLookup.Remove(del);
		}
	}

	public void TriggerEvent(GameEvent e)
	{
		EventDelegate del;
		if (_delegates.TryGetValue(e.GetType(), out del))
		{
			del.Invoke(e);
		}
	}
}