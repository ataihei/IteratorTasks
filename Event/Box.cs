namespace System
{
    /// <summary>
    /// 値型を参照型でただラップするだけのクラス。
    /// Unity iOS で、値型の virtual メンバー呼び出しの AOT JIT エラーがあまりにもつらいので。
    /// </summary>
    /// <typeparam name="T">ラップしたい値型。</typeparam>
    public sealed class Box<T>
        where T : struct
    {
        /// <summary>
        /// ラップしている値。
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 空インスタンス生成。
        /// </summary>
        public Box() { }

        /// <summary>
        /// 値を指定して生成。
        /// </summary>
        /// <param name="value"></param>
        public Box(T value) { Value = value; }

        /// <summary>
        /// 元の値型に変換。
        /// </summary>
        /// <param name="box"></param>
        public static explicit operator T(Box<T> box) => box.Value;

        /// <summary>
        /// 元の値型から変換。
        /// </summary>
        /// <param name="v">ラップしたい値。</param>
        public static explicit operator Box<T>(T v) => new Box<T>(v);
    }

    /// <summary>
    /// <see cref="System.Box{T}"/> 関連拡張。
    /// </summary>
    public static class BoxExtensions
    {
        /// <summary>
        /// 拡張メソッド(後置き記法)で<see cref="System.Box{T}"/>化。
        /// </summary>
        /// <typeparam name="T">ラップしたい値型。</typeparam>
        /// <param name="v">ラップしたい値。</param>
        /// <returns>変換結果。</returns>
        public static Box<T> Box<T>(this T v)
            where T : struct => new Box<T>(v);
    }
}
