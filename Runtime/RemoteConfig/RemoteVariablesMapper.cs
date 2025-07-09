using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace THEBADDEST.RemoteConfigSystem
{


	public enum RemoteVariableType
	{

		Boolean,
		Long,
		Double,
		StringValue

	}

	[Serializable]
	public class RemoteVariable
	{

		public RemoteVariableType type;
		
		public string stringValue;
		public bool booleanValue;
		public int longValue;
		public float doubleValue;
		
		
	}

	public interface IVariablesMapper
	{

		public IDictionary<string, object> GetDefaultValues();

		public void SetValues(IDictionary<string, object> variables);

	}


	public abstract class RemoteVariablesMapper : ScriptableObject, IVariablesMapper
	{

		[SerializeField] protected SerializeDictionary<string, object, RemoteVariable> m_DefaultVariables;
		protected IDictionary<string, object> defaultVariables => m_DefaultVariables;

		public IDictionary<string, object> GetDefaultValues()
		{
			IDictionary<string, object> defaultValues = new Dictionary<string, object>();
			foreach (var resultPair in defaultVariables)
			{
				if (resultPair.Value is RemoteVariable remoteVariable)
				{
					object result = ConvertSoValueToPrimitive(remoteVariable);
					defaultValues.Add(resultPair.Key, result);
				}
			}

			return defaultValues;
		}

		public void SetValues(IDictionary<string, object> variables)
		{
			foreach (var resultPair in variables)
			{
				if (defaultVariables.TryGetValue(resultPair.Key, out object remoteObject) && remoteObject is RemoteVariable remoteVariable)
				{
					ConvertPrimitiveToSoValue(remoteVariable, resultPair.Value);
				}
			}
		}

		protected abstract void ConvertPrimitiveToSoValue(RemoteVariable remoteVariable, object targetObject);

		protected abstract object ConvertSoValueToPrimitive(RemoteVariable remoteVariable);

		public bool Get<T>(string key, out T resultValue) where T : class
		{
			if (defaultVariables.TryGetValue(key, out object value))
			{
				resultValue = value as T;
				return true;
			}

			resultValue = default;
			return false;
		}
		

	}


}