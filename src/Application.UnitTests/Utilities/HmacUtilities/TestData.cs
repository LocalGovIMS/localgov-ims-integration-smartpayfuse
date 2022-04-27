using System.Collections;
using System.Collections.Generic;

namespace Application.UnitTests.Utilities.HmacUtilities
{
    public class TestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new()
        {
            new object[] { "FC81CC7410D19B75B6513FF413BE2E2762CE63D25BA2DFBA63A3183F796530FC", "1:2:3:A:B:C", "LwjcISVfJCYxZfMbhrjiddUStuEgeDB4tNAU7fQL068=" },
            new object[] { "EE82CF73EE82CF73EE82CF73EE82CF73EE82CF73EE82CF73EE82CF73EE82CF73", "1:2:3:A:B:C", "igDoFKtbCUmmPpBpaupykdon5LODl3b0QopMy1CpSC8=" },
            new object[] { "DC82CF79DC82CF74DC82CF74DC82CF74DC82CF74DC82CF74DC82CF74DC82CF74", "1:2:3:A:B:C", "KZadOZsZuw+VBVygaskrqOAoPO/MrvIqYxYEye1B8Eo=" },
            new object[] { "CC83CC76CC83CC76CC83CC76CC83CC76CC83CC76CC83CC76CC83CC76CC83CC76", "1:2:3:A:B:C", "YszLQebpH940r6hAGJOvbWGJG71SQTIlVcomGzSWcSk=" }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
