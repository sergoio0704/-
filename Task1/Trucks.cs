using System;
using System.Collections.Generic;
using System.Text;

namespace Task1
{
    class Trucks : Cars
    {
        public int passengers;

        public Trucks(int wheels, float speed, bool isWorking, int passengers) : base(wheels, speed, isWorking)
        {
            this.passengers = passengers;
        }

        public override void getValues ()
        {
            base.getValues ();
            Console.WriteLine("Passengers: " + passengers);
        }
    }
}
