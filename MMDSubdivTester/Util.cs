using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXTest
{
	public class VDBDebugger : IDisposable
	{
		TcpClient Tcp;
		NetworkStream Ns;
		List<string> Strings;

		public VDBDebugger()
		{
			string ipOrHost = "127.0.0.1";
			int port = 10000;
			Strings = new List<string>( );
			try
			{
				//TcpClientを作成し、サーバーと接続する
				Tcp = new TcpClient( ipOrHost , port );
				Ns = Tcp.GetStream( );

			}
			catch ( Exception e )
			{

			}
		}

		public void Send( string s )
		{
			//Debug.Write(s);
			if ( s == null || s == "" )
				return;
			Encoding enc = Encoding.UTF8;
			try
			{
				byte[] sendBytes = enc.GetBytes( s );
				Ns?.Write( sendBytes , 0 , sendBytes.Length );
			}
			catch ( Exception e )
			{

			}
		}

		public static string vdb_triangle( float x0 , float y0 , float z0 , float x1 , float y1 , float z1 , float x2 , float y2 , float z2 )
		{
			return String.Format( "t {0} {1} {2} {3} {4} {5} {6} {7} {8}\n" , x0 , y0 , z0 , x1 , y1 , z1 , x2 , y2 , z2 );
		}

		public static string vdb_point( float x , float y , float z )
		{
			string line = String.Format( "p {0} {1} {2}\n" , x , y , z );
			return line;
		}

		public static string vdb_line( float x0 , float y0 , float z0 , float x1 , float y1 , float z1 )
		{
			return String.Format( "l {0} {1} {2} {3} {4} {5}\n" , x0 , y0 , z0 , x1 , y1 , z1 );
		}

		public static string vdb_color( float x0 , float y0 , float z0 , float x1 , float y1 , float z1 )
		{
			return String.Format( "c {0} {1} {2}\n" , x0 , y0 , z0 );
		}

		int vdb_intern( string str )
		{
			for ( int i = 0 ; i < Strings.Count ; i++ )
			{
				if ( Strings[ i ] == str )
				{
					Send( String.Format( "s {0} {1}\n" , i , str ) );
					return i;
				}
			}
			Strings.Add( str );
			int key = Strings.Count;
			Send( String.Format( "s {0} {1}\n" , key , str ) );
			return key;
		}

		public void vdb_label( string s )
		{
			int key = vdb_intern( s );
			Send( $"g {key}\n" );
		}

		public void Dispose()
		{
			Ns.Close( );
			Tcp.Close( );
		}
	}

	public static class Util
	{
		public static Vec3 ToV3(this Vector3 v )
		{
			return new Vec3( v.X , v.Y , v.Z );
		}

		public static Vector3 ToSharpV3(this Vec3 v )
		{
			return new Vector3( v.X , v.Y , v.Z );
		}

		public static float Sin( float f )
		{
			return ( float )Math.Sin( f );
		}

		public static float ASin( float f )
		{
			return ( float )Math.Asin( f );
		}

		public static float Cos( float f )
		{
			return ( float )Math.Cos( f );
		}

		public static float ACos( float f )
		{
			return ( float )Math.Acos( f );
		}

		public static float Abs( this float f )
		{
			return Math.Abs( f );
		}

		public static float Rad( this float f )
		{
			return f / 180.0f * ( float )Math.PI;
		}

		public static float Deg( this float f )
		{
			return f / ( float )Math.PI * 180.0f;
		}

		public static float AddDeg( this float deg , float delta )
		{
			float added = deg + delta;
			if ( added < 0 )
			{
				added = -added;
				return 360.0f - added % 360.0f;
			}
			return added % 360.0f;
		}

		public static float Clamp( this float v , float min , float max )
		{
			return Math.Min( Math.Max( v , min ) , max );
		}

		public static float AddDegClamp( this float deg , float delta )
		{
			float added = deg + delta;

			return added.Clamp( 180.5f , 359.5f );
		}

		public const float ZeroTolerancef = 1e-06f;

		public static void ArrayFullCopy( this Array array , Array target )
		{
			Array.Copy( array , target , array.Length );
		}

		public static bool IsNearlyEqual( this float v , float other , float eps = float.Epsilon )
		{
			return ( v - other ).Abs( ) < eps;
		}

		public static bool IsNearlyZero( this float v , float eps = float.Epsilon )
		{
			return v.IsNearlyEqual( 0.0f , eps );
		}

		public static int FloorToInt( this float v )
		{
			return (int)Math.Floor( v );
		}

		public static float Float( this string s )
		{
			return float.Parse( s );
		}

		public static int Int( this string s )
		{
			return int.Parse( s );
		}

		public static void DebugWrite( this string s )
		{
			//if ( frameCount == 500 )
			{
			Debug.WriteLine( " " + s );
			}
		}

		//public static IEnumerable<Vector3> ParseCSV( IEnumerable<string> lines )
		//{
		//	//Vertex,0,-5,-5,-5 ,-1
		//	foreach ( string item in lines )
		//	{
		//		string[] csv = item.Split( ',' );
		//		yield return new Vector3( csv[ 2 ].Float( ) , csv[ 3 ].Float( ) , csv[ 4 ].Float( ) );
		//	}
		//}

		public static int[] ParseFaceCSV( string line )
		{

			string[] csv = line.Split( ',' );
			return new int[] { csv[ 3 ].Int( ) , csv[ 4 ].Int( ) , csv[ 5 ].Int( ) };
		}

		public static IEnumerable<int[]> ParseFaceCSVAll( IEnumerable<string> lines )
		{
			//Face,"箱",1,2,3,0
			foreach ( string item in lines )
			{
				yield return ParseFaceCSV( item );
			}
		}

		//public static Option<T> MinValue<T>( this IEnumerable<T> items , Func<T , float> sel )
		//{
		//	float minValue = float.MaxValue;
		//	T minItem = default(T);
		//	bool hasValue = false;
		//	foreach ( var item in items )
		//	{
		//		float val = sel( item );
		//		if ( minValue > val )
		//		{
		//			minValue = val;
		//			minItem = item;
		//			hasValue = true;
		//		}
		//	}
		//	if ( hasValue )
		//	{
		//		return Option.Return(minItem);
		//	}
		//	return Option.Return<T>( );
		//}

		public static IEnumerable<T> Range<T>( this IEnumerable<T> ts , int start , int count )
		{
			return ts.Skip( start ).Take( count );
		}

		public static IEnumerable<T> TakeIndice<T>( this T[] items , IEnumerable<int> indice )
		{
			foreach ( var item in indice )
			{
				yield return items[ item ];

			}
		}

		public static int FirstIndex<T>( this List<T> items , System.Predicate<T> f )
		{
			var c = items.Count;
			for ( int i = 0 ; i < c ; i++ )
			{
				if ( f( items[ i ] ) )
					return i;
			}
			return -1;
		}

		public static int FirstIndex<T>( this T[] items , System.Predicate<T> f )
		{
			var c = items.Length;
			for ( int i = 0 ; i < c ; i++ )
			{
				if ( f( items[ i ] ) )
					return i;
			}
			return -1;
		}

		public static string ConcatStr<T>( this IEnumerable<T> items )
		{
			StringBuilder builder = new StringBuilder( );
			foreach ( T item in items )
			{
				builder.Append( item );
			}
			return builder.ToString( );
		}

		public static void WriteFile( this IEnumerable<string> line , string path )
		{
			File.WriteAllLines( path , line );
		}

	}
/*
	public class HitResult
	{
		public bool IsHit
		{
			get;
		}

		public Vector3 HitPosition
		{
			get;
		}

		public string Info
		{
			get;
		}

		public HitResult( bool isHit , Vector3 pos )
		{
			IsHit = isHit;
			HitPosition = pos;
		}

		public HitResult( Vector3 pos , string info )
		{
			IsHit = true;
			HitPosition = pos;
			Info = info;
		}

		public static HitResult Null
		{
			get
			{
				return new HitResult( false , Vector3.Zero );
			}
		}

	}

	public class RayWrap
	{
		public Vector3 From
		{
			get;
			private set;
		}

		public Vector3 To
		{
			get;
			private set;
		}

		float Length;

		public Vector3 Dir
		{
			get;
			private set;
		}

		Ray Ray;

		public RayWrap( Vector3 from , Vector3 to )
		{
			From = from;
			To = to;
			Vector3 vec = ( to - from );
			Length = vec.Length( );
			vec.Normalize( );
			Dir = vec;
			Ray = new Ray( From , Dir );
		}

		public RayWrap( Ray ray , float length = 100 )
		{
			From = ray.Position;
			Dir = ray.Direction;
			Length = length;
			Ray = ray;
			To = From + Dir * Length;
		}

		public string RayStr
		{
			get
			{
				return VDBDebugger.vdb_line( From.X , From.Y , From.Z , To.X , To.Y , To.Z );
			}
		}

		public void Extend( float len )
		{
			Length += len;
			To = From + Dir * Length;
		}

		public HitResult IntersectFace( Face face )
		{
			Vector3 p1 = face.P1;
			Vector3 p2 = face.P2;
			Vector3 p3 = face.P3;
			bool intersects = Ray.Intersects( ref p1 , ref p2 , ref p3 , out float dist );
			if ( intersects )
			{
				return new HitResult( From + Dir * dist , face.MatName.Replace( "\"" , "" ) );
			}
			return HitResult.Null;

		}
		public override string ToString()
		{
			return RayStr;
		}
	}

	public class Face
	{
		public string MatName
		{
			get;
		}

		public Vector3 P1
		{
			get;
			private set;
		}

		public Vector3 P2
		{
			get;
			private set;
		}

		public Vector3 P3
		{
			get;
			private set;
		}

		public Vector3 P1O
		{
			get;
			private set;
		}

		public Vector3 P2O
		{
			get;
			private set;
		}

		public Vector3 P3O
		{
			get;
			private set;
		}

		public Vector3 AB
		{
			get;
			private set;
		}

		public Vector3 BC
		{
			get;
			private set;
		}

		public Vector3 CA
		{
			get;
			private set;
		}

		public Vector3 BaryCentric;

		public float AverageZ
		{
			get
			{
				return P1.Z + P2.Z + P3.Z;
			}
		}

		public void SetP1( Vector3 loc )
		{
			Vector3 delta = loc - P1;
			P1 -= loc;
			P2 += delta;
			P3 += delta;
			RecalcEdge( );
		}

		public Face( Vector3 p1 , Vector3 p2 , Vector3 p3 , string matName )
		{
			P1 = p1;
			P2 = p2;
			P3 = p3;
			P1O = p1;
			P2O = p2;
			P3O = p3;
			Vector3 n1 = P1 - P2;
			Vector3 n2 = P1 - P3;
			n1.Normalize( );
			n2.Normalize( );
			Normal = Vector3.Cross( n2 , n1 );
			BaryCentric = ( P1 + P2 + P3 ) / 3.0f;
			RecalcEdge( );
			MatName = matName;
		}

		private void RecalcEdge()
		{
			AB = P2 - P1;
			BC = P3 - P2;
			CA = P1 - P3;
		}

		public void Update( Matrix matrix )
		{
			P1 = matrix.TransByMat( P1O ).ToV3( );
			P2 = matrix.TransByMat( P2O ).ToV3( );
			P3 = matrix.TransByMat( P3O ).ToV3( );
			RecalcEdge( );

		}

		public string TriString
		{
			get
			{
				return VDBDebugger.vdb_triangle(
				  P1.X , P1.Y , P1.Z ,
				  P2.X , P2.Y , P2.Z ,
				  P3.X , P3.Y , P3.Z
				  );
			}
		}

		public Vector3 Normal
		{
			get;
		}

		public override string ToString()
		{
			return P1.ToString( ) + " " + P2.ToString( ) + " " + P3.ToString( );
		}

	}
*/
}
