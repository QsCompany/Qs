using System.Runtime.InteropServices;

[ComImport, Guid("E436EBB3-524F-11CE-9F53-0020AF0BA770")]
class FilgraphManager
{

}
[Guid("56A868B1-0AD4-11CE-B03A-0020AF0BA770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
interface IMediaControl // cannot list any base interfaces here 
{
    // Note that the members of IUnknown and Interface are NOT
    // listed here 
    //
    void Run();

    void Pause();

    void Stop();

    void GetState([In] int msTimeout, [Out] out int pfs);

    void RenderFile(
    [In, MarshalAs(UnmanagedType.BStr)] string strFilename);

    void AddSourceFilter(
    [In, MarshalAs(UnmanagedType.BStr)] string strFilename,
    [Out, MarshalAs(UnmanagedType.Interface)] out object ppUnk);

    [return: MarshalAs(UnmanagedType.Interface)]
    object FilterCollection();

    [return: MarshalAs(UnmanagedType.Interface)]
    object RegFilterCollection();

    void StopWhenReady();
}
namespace Qs
{
    public class MyClass
    {
        private int y;
    }
    public class Errord
    {
        public static implicit operator MyClass(Errord t)
        {
            return new MyClass();
            //throw new Exception();
            //return 12;
        }

        public static void Tester()
        {
            object z = new Errord();
            var t = (MyClass) (Errord) z;
            var @class = z as MyClass;
        }

        public Errord()
        {

            
        }

        void vlo()
        {
        }
    }
}