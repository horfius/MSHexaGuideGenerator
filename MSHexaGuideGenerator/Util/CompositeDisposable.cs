using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSHexaGuideGen.Util
{
    public class CompositeDisposable : IDisposable
    {
        private List<IDisposable> disposables = new List<IDisposable>();

        public CompositeDisposable() { }

        public void AddDisposable(IDisposable disposable) => disposables.Add(disposable);

        public void AddRangeDisposables(IEnumerable<IDisposable> disposables)
        {
            foreach (IDisposable disposable in disposables) 
                AddDisposable(disposable);
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
            disposables.Clear();
        }
    }
}
