using System;

namespace Task1
{   enum Models {AUDI, VOLVO, NISSAN, PORSHE, VOLKSWAGEN, FORD}
    class Program
    {
        static void Main (string[] args)
        {
            Cars audi = new Cars (4, 123.5f, false);
            audi.model = Models.AUDI;
//            Console.WriteLine (audi.wheels);

//            audi.setValues(228.1f, true);
            audi.getValues ();

            Cars volvo = new Cars (6, 228.1f, true);
            volvo.model = Models.VOLVO;
            volvo.wheels = 6;
//            volvo.setValues (123.5f, false);
            volvo.getValues ();

            Trucks man = new Trucks (8, 134.5f, true, 3);
            man.getValues ();
            Console.WriteLine(man.passengers);
        }
    }
}
