namespace AutoMapper
{
    using System;

    public class TypeMapCreatedEventArgs : EventArgs
    {
        public TypeMap TypeMap { get; private set; }

        public TypeMapCreatedEventArgs(TypeMap typeMap)
        {
            TypeMap = typeMap;
        }
    }
}