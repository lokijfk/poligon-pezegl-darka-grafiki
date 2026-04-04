using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poligon_pezeglądarka_grafiki.Model.Interface;
/// <summary>
/// interfejs do pobierania danych z pliku ini, który jest używany do sprawdzania czy jest już uruchomiona inna instancja programu
/// docelowo do testowania DI, ale na razie nie widzę zastosowanie dla DI i na razie chyba odpuszczę  to rozwiązanie, ale zostawię ten interfejs na wszelki wypadek, może kiedyś się przyda
/// </summary>
interface IBrokerIni
{
    static abstract BrokerIni GetBroker();
}
