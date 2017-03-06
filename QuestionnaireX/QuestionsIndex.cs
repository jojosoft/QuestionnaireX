/*
 * Author: Johannes Schirm, MPI for Biological Cybernetics
 * Written on behalf of Betty Mohler.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionnaireX
{
    static class QuestionsIndex
    {
        /// <summary>
        /// This is an index you need to register with if you want your own question form to be used by the main logic.
        /// It's as simple as adding a new entry that maps from the question type keyword (input files) to the C# type that you've created.
        /// Whenever the main logic detects the question type keyword, it shows the corresponding form and gets the result by calling ToString().
        /// So please make sure your new form does fulfill the following requirements:
        ///     1. Being a direct sub-class of System.Windows.Form
        ///     2. Having a constructor that accepts a single FileHelpers.DataRow for parsing the parameters
        ///     3. Overriding the ToString() method for enabling the main logic to get your result
        ///     4. Setting DialogResult to DialogResult.OK and closing your form if the user finished answering
        /// </summary>
        public static Dictionary<string, Type> INDEX = new Dictionary<string, Type>()
        {
            { "Scale", typeof(Slider) },
            { "Buttons", typeof(Buttons) },
            { "Instruction", typeof(Instructions) },
            { "ButtonsImage", typeof(ButtonsImage) }
        };
    }
}
