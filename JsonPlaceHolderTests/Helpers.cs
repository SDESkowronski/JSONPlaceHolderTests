using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonPlaceHolderTests
{
    static public class Helpers
    {
        static public void AssertCollectionsAreEqual(ICollection expected, ICollection actual, string message = "")
        {
            if (expected.Count != actual.Count) 
                throw new Exception($"Collection Count Mismatch: expected: {expected.Count} actual: {actual.Count} - {message}");

            var enumeratorExpected = expected.GetEnumerator();
            var enumeratorActual = actual.GetEnumerator();
            int index = 0;
            while(enumeratorExpected.MoveNext() && enumeratorActual.MoveNext())
            {
                if (!enumeratorExpected.Current.Equals(enumeratorActual.Current))
                    throw new Exception($"Collection Differs at Index: {index} - {message}");
                index++;
            }
        }
    }
}
