namespace Frontend.WebApp.Models
{
    public class Pagina<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePrev => PaginaActual > 1;
        public bool TieneNext => PaginaActual < TotalPaginas;
        public string Url { get; set; } = "";

        public List<(int Numero, bool Actual)> Paginas => Enumerable.Range(1, TotalPaginas)
            .Select(n => (Numero: n, Actual: n == PaginaActual))
            .ToList();
    }
}
