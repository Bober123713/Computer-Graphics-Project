using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CG1;

public class Filter(string name, Action<WriteableBitmap> data)
{
    public string? Name { get; set; } = name;
    private Action<WriteableBitmap> Data { get; init; } = data;

    public void Apply(WriteableBitmap source)
    {
        Data.Invoke(source);
    }
}
