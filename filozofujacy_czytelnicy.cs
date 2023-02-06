using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

//wykorzystałam implementację ucztujących filozofów z wykładu

//Ucztujący czytelnicy - filozofowie czytają przy jedzeniu; 
//mają do dyspozycji k książek;
//każdy przed rozpoczęciem jedzenia bierze jedną z książek - za każdym razem musi to być inna książka;
//filozofowie mogą usiąść przy innym miejscu (wybieranym losowo); widelce biorą zawsze z sąsiedztwa wybranego nakrycia;
namespace filozofowie
{
    public class Filozof
    {
        int num;
        SemaphoreSlim[] widelce;
        SemaphoreSlim[] ksiazki;
        SemaphoreSlim stol;
        SemaphoreSlim[] miejsca;
        int miejsce;
        int lewy, prawy;
        int ktora_ksiazka = -1;
        int poprzednia = -2;
        Random rng = new Random();
        protected int k; //książek

        public Filozof(int num, SemaphoreSlim[] widelce, SemaphoreSlim[] ksiazki, SemaphoreSlim stol, SemaphoreSlim[] miejsca, int k) //k książek
        {
            this.num = num;
            this.widelce = widelce;
            this.ksiazki = ksiazki;
            this.stol = stol;
            this.k = k;
            this.miejsca = miejsca;
            lewy = miejsce % widelce.Length;
            prawy = (miejsce + 1) % widelce.Length;

            ktora_ksiazka = rng.Next(0, k);

            Thread.Sleep(50);
            miejsce = rng.Next(0, miejsca.Length);
        }
        public void Mysl()
        {
            Console.WriteLine($"Filozof {num} myśli po zasiąściu przy miejscu {miejsce}...");
            //coś krótko myśli, ale też nie widzę szczególnej potrzeby wydłużania tego procesu
            Console.WriteLine($"Filozof {num} zgłodniał!");
        }
        public void Jedz()
        {
            Console.WriteLine($"Filozof {num} zaczyna jeść {lewy} i {prawy} widelcem...");
            Thread.Sleep(rng.Next(500, 1000)); //tutaj jest potrzeba bo używane są widelce, jak rozumiem
            Console.WriteLine($"Filozof {num} skończył jeść {lewy} i {prawy} widelcem!  Opuszcza miejsce {miejsce}");
        }
        public void Czytaj()
        {
            Console.WriteLine($"Filozof {num} zaczyna czytać {ktora_ksiazka} książkę...");
            Thread.Sleep(rng.Next(500, 1000)); //tutaj widzę potrzebę wydłużenia procesu ponieważ książki nie rwiemy
            Console.WriteLine($"Filozof {num} skończył czytać {ktora_ksiazka} książkę!");
        }
        public void Dzialanie()
        {
            for(; ; )
            {
                stol.Wait();

                Thread.Sleep(500);
                miejsce = rng.Next(0, miejsca.Length); 
                miejsca[miejsce].Wait();
                lewy = miejsce % widelce.Length;
                prawy = (miejsce + 1) % widelce.Length;

                Mysl();

                while (poprzednia == ktora_ksiazka) //i zapobiega to przeczytaniu od razu tej samej książki i pozwala generować dość dobrze losowo książki
                {
                    Thread.Sleep(50);
                    ktora_ksiazka = rng.Next(0, k);
                }

                ksiazki[ktora_ksiazka].Wait();
                Czytaj();

                widelce[lewy].Wait();
                widelce[prawy].Wait();

                Jedz();

                widelce[prawy].Release();
                widelce[lewy].Release();

                ksiazki[ktora_ksiazka].Release(); //zakładam że ucztujący, czytający filozof zaczyna czytać przed jedzeniem i kończy zaraz po, a potem wstaje od stołu
                poprzednia = ktora_ksiazka;
                miejsca[miejsce].Release();

                stol.Release();
            }
        }
    }

    public class Biesiada
    {
        public static void Biesiaduj(int k) //k książek
        {
            int ile_filozofow = 5;
            SemaphoreSlim[] widelce = new SemaphoreSlim[ile_filozofow];
            SemaphoreSlim[] ksiazki = new SemaphoreSlim[k];
            SemaphoreSlim[] miejsca = new SemaphoreSlim[ile_filozofow - 1]; //zakładam że miejsc jest tyle ilu jednocześnie dopuszczamy jednocześnie do stołu filozofów, czyli poza jednym

            for (int i = 0; i < ile_filozofow; i++)
            {
                widelce[i] = new SemaphoreSlim(1, 1);
            }

            for (int i = 0; i < k; i++)
            {
                ksiazki[i] = new SemaphoreSlim(1);
            }

            for (int i = 0; i < miejsca.Length; i++)
            {
                miejsca[i] = new SemaphoreSlim(1);
            }

            SemaphoreSlim stol = new SemaphoreSlim(ile_filozofow - 1, ile_filozofow - 1);

            Filozof[] filozofowie = new Filozof[ile_filozofow];
            Thread[] watki = new Thread[ile_filozofow];

            for (int i = 0; i < ile_filozofow; i++)
            { 
                filozofowie[i] = new Filozof(i, widelce, ksiazki, stol, miejsca, k);
                watki[i] = new Thread(filozofowie[i].Dzialanie);
            }

            foreach (var watek in watki) watek.Start();
            foreach (var watek in watki) watek.Join();
        }
        public static void Main()
        {
            Biesiaduj(10);
        }
    }
}
