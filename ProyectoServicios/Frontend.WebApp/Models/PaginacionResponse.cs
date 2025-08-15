namespace Frontend.WebApp.Models
{
    public class PaginacionResponse
    {
        public int CurrentPage { get; set; }  
        public int PageSize { get; set; }     
        public int TotalRecords { get; set; }  
        public int TotalRegistros { get; set; }
        public List<Cliente> Items { get; set; } = new List<Cliente>();
    }
}
