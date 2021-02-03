//#define MEMORY_LOG

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace ProgrammingLanguageNr1
{
    public enum ReturnValueType
    {
        NUMBER, VOID, STRING, BOOL, ARRAY, RANGE, UNKNOWN_TYPE
    }

	public struct VoidType {
		public static VoidType voidType = new VoidType();
	};

	public struct UnknownType {
		public static UnknownType unknownType = new UnknownType();
	};

	public class ReturnValueConversions {

		public static T SafeUnwrap<T>(object[] args, int index) {
			if(args[index].GetType() == typeof(T)) {
				return (T)args[index];
			}
			else {
				throw new Error("Arg " + index + " is of wrong type (" + ReturnValueConversions.PrettyObjectType(args[index].GetType()) + 
				                "), should be " + ReturnValueConversions.PrettyObjectType(typeof(T)));
			}
		}

//		static Dictionary<Type, ReturnValueType> typeToReturnValueType = new Dictionary<Type, ReturnValueType>() {
//			{ typeof(float), ReturnValueType.NUMBER },
//			{ typeof(string), ReturnValueType.STRING },
//			{ typeof(bool), ReturnValueType.BOOL },
//			{ typeof(object[]), ReturnValueType.ARRAY },
//			{ typeof(Range), ReturnValueType.RANGE },
//			{ typeof(VoidType), ReturnValueType.VOID },
//		};
		
		public static ReturnValueType SystemTypeToReturnValueType(Type t) {
			if(t == typeof(SortedDictionary<KeyWrapper,object>)) {
				return ReturnValueType.ARRAY;
			}
			else if(t == typeof(float)) {
				return ReturnValueType.NUMBER;
			}
			else if(t == typeof(bool)) {
				return ReturnValueType.BOOL;
			}
			else if(t == typeof(string)) {
				return ReturnValueType.STRING;
			}
			else if(t == typeof(Range)) {
				return ReturnValueType.RANGE;
			}
			else if(t == typeof(VoidType)) {
				return ReturnValueType.VOID;
			}
			else {
				return ReturnValueType.UNKNOWN_TYPE;
			}
//			ReturnValueType retValType = ReturnValueType.UNKNOWN_TYPE;
//			typeToReturnValueType.TryGetValue(t, out retValType);
//			return retValType;
		}


		static Dictionary<Type, string> typeToString = new Dictionary<Type, string>() {
			{ typeof(float), "number" },
			{ typeof(string), "string" },
			{ typeof(bool), "bool" },
			{ typeof(object[]), "prim-array" },
		};
		
		static public string PrettyObjectType (Type t) 
		{
			if(t == typeof(SortedDictionary<KeyWrapper,object>)) {
				return "array";
			}
			
			string s;
			if(typeToString.TryGetValue(t, out s)) {
				return s;
			}
			
			return "unknown";
		}

		static public string PrettyStringRepresenation(object o) {

			//Console.WriteLine("Will pretty print object " + o.ToString() + " of type " + o.GetType());

			if(o.GetType() == typeof(string)) {
				//return "'" + (string)o + "'";
				return (string)o;
			}
			else if(o.GetType() == typeof(bool)) {
				return ((bool)o) ? "true" : "false";
			}
			else if(o.GetType() == typeof(float)) {
				return ((float)o).ToString(CultureInfo.InvariantCulture);
			}
			else if(o.GetType() == typeof(int)) {
				return o.ToString() + "i";
			}
			else if(o.GetType() == typeof(object[])) {
				return MakePrimitiveObjectArrayString((object[])o);
			}
			else if(o is Range) {
				return ((Range)o).ToString();
			}
			else if(o is KeyWrapper) {
				return "{" + ((KeyWrapper)o).value.ToString() + "}";
			}
//			else if(o is ReturnValueType) {
//				return o.ToString();
//			}
			else if(o is SortedDictionary<KeyWrapper,object>) {
				return MakeArrayString(o as SortedDictionary<KeyWrapper,object>);
			}
			else if(o.GetType() == typeof(UnknownType)) {
				return o.ToString();
			}

			throw new Error("Can't pretty print " + o.ToString() + " of type " + o.GetType());
		}

		static string MakePrimitiveObjectArrayString (object[] array)
		{
			if(array != null) {
				StringBuilder s = new StringBuilder();
				s.Append("@[");
				int count = array.Length;
				int emergencyBreak = 0;
				for(int i = 0; i < array.Length; i++) {
					s.Append(PrettyStringRepresenation(array[i]));
					count--;
					if(count > 0) {
						s.Append(", ");
					}
					emergencyBreak++;
					if (emergencyBreak > 10) {
						s.Append ("...");
						break;
					}
				}
				s.Append("]");
				return s.ToString();
			}
			else {
				return "";
			}
		}

		static string MakeArrayString (SortedDictionary<KeyWrapper,object> array)
		{
			if(array != null) {
				StringBuilder s = new StringBuilder();
				s.Append("[");
				int count = array.Count;
				int emergencyBreak = 0;
				//Console.WriteLine ("Keys in array: " + string.Join (", ", m_arrayValue.Keys.Select (k => "Key " + k.ToString () + " of type " + k.m_returnType).ToArray()));
				foreach(var key in array.Keys) {
					//Console.WriteLine ("- Looking up key " + key);
					s.Append(/*PrettyStringRepresenation(key.value) + "=>" + */PrettyStringRepresenation(array[key]));
					count--;
					if(count > 0) {
						s.Append(", ");
					}
					emergencyBreak++;
					if (emergencyBreak > 10) {
						s.Append ("...");
						break;
					}
				}
				s.Append("]");
				return s.ToString();
			}
			else {
				return "";
			}
		}

		public static object ChangeTypeBasedOnReturnValueType (object obj, ReturnValueType type)
		{
			switch(type) {
				case ReturnValueType.STRING:
					return PrettyStringRepresenation(obj);
				case ReturnValueType.NUMBER:
					return ConvertToNumber(obj);
				case ReturnValueType.RANGE:
					return (Range)obj;
				case ReturnValueType.ARRAY:
					if(obj.GetType() == typeof(object[]))
						return obj;
					else if(obj.GetType() == typeof(SortedDictionary<KeyWrapper,object>))
						return obj;
					else if(obj.GetType() == typeof(Range))
						return obj;
					else if(obj.GetType() == typeof(string)) {
                        SortedDictionary<KeyWrapper, object> a = new SortedDictionary<KeyWrapper,object>();
						string s = (string)obj;
						for(int i = 0; i < s.Length; i++)
							a.Add(new KeyWrapper((float)i), s.Substring(i, 1));
						return a;
					}
					
					throw new Error("Can't convert " + obj.ToString() + " to an array");
				case ReturnValueType.BOOL:
					return ConvertToBool(obj);
				case ReturnValueType.UNKNOWN_TYPE:
					return obj;
			}
			
			throw new Error("Can't change type from " + obj.GetType() + " to " + type);
		}

		public static float ConvertToNumber(object o)
		{
			if(o.GetType() == typeof(float))
				return (float)o;
			else if(o.GetType() == typeof(int))
				return (float)(int)o;
			else if(o.GetType() == typeof(string)) {
				try
                {
					return (float)Convert.ToDouble((string)o, CultureInfo.InvariantCulture);
				}
				catch(System.FormatException)
                {
					throw new Error("Can't convert " + o.ToString() + " to a number");
				}
			}

			throw new Error("Can't convert value " + o + " of type " + PrettyObjectType(o.GetType()) + " to number");
		}

		public static bool ConvertToBool(object o)
		{
			if(o.GetType() == typeof(bool))
				return (bool)o;
			else if(o.GetType() == typeof(float))
				return (float)o != 0f;
			else if(o.GetType() == typeof(int))
				return (int)o != 0;
			
			throw new Error("Can't convert value " + o + " of type " + PrettyObjectType(o.GetType()) + " to bool");
		}
	}


	public struct KeyWrapper : IComparable<KeyWrapper>
	{
		public KeyWrapper(object o) {
			this.value = o;
		}

		public object value;

		public override int GetHashCode () {
			if (this.value.GetType() == typeof(int))
				return (int)this.value;
			else if (this.value.GetType() == typeof(float))
				return (int)(float)this.value;
			else if (this.value.GetType() == typeof(bool)) {
				if ((bool)this.value)
					return 9998;
				else
					return 9999;
			}
			else if (this.value.GetType() == typeof(string))
				return 10000 + ((string)value).GetHashCode () % 10000;
			else
				return 20000 + base.GetHashCode () % 10000;
		}

		public int CompareTo(KeyWrapper pOther) {
			int diff = this.GetHashCode() - pOther.GetHashCode();
			//Console.WriteLine ("Comparing " + this.ToString () + " with " + pOther.ToString () + ", diff = " + diff);
			return diff;
		}
	}
}

