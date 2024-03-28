namespace ToDoWPFApp {
    internal class ToDoItem {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool Checked { get; set; }
        public static int HighestId = -1;
        public ToDoItem( string title, string content, bool @checked ) {
            Id = ++HighestId;
            Title = title;
            Content = content;
            Checked = @checked;
        }
        public void SetId( int id ) { Id = id; }
        public override string ToString() => Id + "," + Title + "," + Content + "," + Checked;
    }
}
