namespace G4.QueryFramework.Attributes {
    public class ForeignConfigureAttribute : Attribute {

        public string JoinType { get; set; } = "INNER";
        public string[] KeysIn { get; set; }
        public string[] KeysOut { get; set; }

        public ForeignConfigureAttribute(string[] KeysIn, string[] KeysOut, string joinType = "INNER") {
            this.JoinType = joinType;
            this.KeysIn = KeysIn;
            this.KeysOut = KeysOut;
        }

    }
}
