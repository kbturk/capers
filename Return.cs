namespace capers;

public class Return: Exception {
    public object val;

    public Return (object val) {
        this.val = val;
    }
}
