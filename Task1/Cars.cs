using System;
using System.Collections.Generic;
using System.Text;

namespace Task1
{
    class Cars
    {
        public int wheels = 4;
        private float speed;
        protected bool isWorking = true;
        public Models model;

        public void setValues(float speed, bool isWorking)
        {
            this.speed = speed;
            this.isWorking = isWorking;   
        }

        public virtual void getValues()
        {
            Console.WriteLine("Car speed is: " + this.speed + ", car is working: " + this.isWorking);
        }

        public Cars(int wheels, float speed, bool isWorking)
        {
            this.speed = speed;
            this.wheels = wheels;
            this.isWorking = isWorking;
        }

        public Cars() {}

    }
}
