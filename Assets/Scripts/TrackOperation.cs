public class TrackOperation
{
    public string type; // int || bool || string
    public string colour; // red || green || blue
    public int valueI;
    public bool valueB;
    public string valueS;
    public bool typeError = false;
    public char operation; // + || - || * || =
    public char compOperation = default; // = || > || <
}