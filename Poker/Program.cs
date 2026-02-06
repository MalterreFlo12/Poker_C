using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Poker  // demander si le fait 
{
    class Program
    {
        // -----------------------
        // DECLARATION DES DONNEES
        // -----------------------
        // Importation des DL (librairies de code) permettant de gérer les couleurs en mode console
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, int wAttributes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(uint nStdHandle);
        static uint STD_OUTPUT_HANDLE = 0xfffffff5;
        static IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        // Pour utiliser la fonction C 'getchar()' : sasie d'un caractère
        [DllImport("msvcrt")]
        static extern int _getch();

        //-------------------
        // TYPES DE DONNEES
        //-------------------

        // Fin du jeu
        public static bool fin = false;

        // Codes COULEUR
        public enum couleur { VERT = 10, ROUGE = 12, JAUNE = 14, BLANC = 15, NOIRE = 0, ROUGESURBLANC = 252, NOIRESURBLANC = 240 }; // liste pour que la consol comprenne les couleur 

        // Coordonnées pour l'affichage
        public struct coordonnees
        {
            public int x;
            public int y;
        }

        // Une carte
        public struct carte
        {
            public char valeur;
            public int famille;
        };

        // Liste des combinaisons possibles
        public enum combinaison { RIEN, PAIRE, DOUBLE_PAIRE, BRELAN, QUINTE, FULL, COULEUR, CARRE, QUINTE_FLUSH };

        // Valeurs des cartes : As, Roi,...
        public static char[] valeurs = { 'A', 'R', 'D', 'V', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };

        // Codes ASCII (3 : coeur, 4 : carreau, 5 : trèfle, 6 : pique)
        public static char[] familles = { '\u2665', '\u2666', '\u2663', '\u2660' };

        // Numéros des cartes à échanger
        public static int[] echange = { 0, 0, 0, 0 };

        // Jeu de 5 cartes
        public static carte[] MonJeu = new carte[5]; // MonJeu est l'amplacement réservé pour les 5 cartes et unJeu est la variable grâce à laquelle on remplie le jeu (MonJeu)

        //----------
        // FONCTIONS
        //---------- 
        // Génère aléatoirement une carte : {valeur;famille}
        // Retourne une expression de type "structure carte"
        private static Random rand = new Random();
        public static carte tirage()
        {
            carte carte;
            int valeur = rand.Next(0,13);
            int famille = rand.Next(0,4);// tirage aléatoir / le .Next permet determiner la tranche de numéro voulue et d'en recupéré un aléatoirement 
            carte.valeur = valeurs[valeur];// la valeur de la carte est = a l'indice dans la liste valeurs 
            carte.famille = familles[famille];// ... dans la liste familles
            return carte;
        }

        // Indique si une carte est déjà présente dans le jeu
        // Paramètres : une carte, le jeu 5 cartes, le numéro de la carte dans le jeu
        // Retourne un entier (booléen)
        public static bool carteUnique(carte uneCarte, carte[] unJeu, int numero)//numéro est l'emplacement de la carte la carte 1,2,3,4 ou 5...
        {
            if (uneCarte.valeur == unJeu[numero].valeur && uneCarte.famille == unJeu[numero].famille)//
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Calcule et retourne la COMBINAISON (paire, double-paire... , quinte-flush)
        // pour un jeu complet de 5 cartes.
        // La valeur retournée est un élement de l'énumération 'combinaison' (=constante)
        public static combinaison cherche_combinaison(ref carte[] unJeu)
        {
            int[] similaire = { 0, 0, 0, 0, 0 }; // Nombre de valeurs similaires dans le jeu pour chaque carte
            int[] mfamille = { 0, 0, 0, 0, 0 };// Nombre de carte aillant le même famille 
            char[,] quintes ={ {'X','V','D','R','A'},{'9','X','V','D','R'},{'8','9','X','V','D'},{'7','8','9','X','V'} };// les possibilité de quinte 

            combinaison resultat = combinaison.RIEN;// variable que l'on va appeler pour afficher le resultat a l'utilisateur 

            for (int i = 0; i < 5; i++) // prend une carte en repère pour la comparer au autres
            {
                for(int j = 0; j < 5; j++) // autre carte comparé avec la carte repère 
                {
                    if (unJeu[i].valeur == unJeu[j].valeur) // si deux carte sont identique, alros la valeur de la carte repéré fait +1 dans le tableau 'similaire' 
                    {
                        similaire[i]++;
                    }
                    if (unJeu[i].famille == unJeu[j].famille)// si deux carte sont de la même famille alors , alros la valeur de la carte repéré fait +1 dans le tableau 'mfamille'
                    {
                        mfamille[i]++;
                    }
                }
            }
            int compte = 0;//instensiation d'un conteur
            for (int i = 0;i < similaire.Length;i++) // i passe le tableau "similaire" en vue 
            {
                if(similaire[i] == 2) // si l'indice i du tableau "similaire" est = a 2 alors il y a une paire
                {
                    compte = compte + 1; 
                    resultat = combinaison.PAIRE;
                }
                
                if (compte/2 == 2) //le comteur est /2 si il est égale a deux alors il y a une double paire (le compteur sera égale a quatre car il aurra rencontré 4x2) 
                {
                    resultat = combinaison.DOUBLE_PAIRE;
                }

                if (similaire[i] == 4)
                {
                    resultat = combinaison.CARRE;
                }
                if (similaire[i] == 3)
                {
                    resultat = combinaison.BRELAN;
                }
                if (resultat == combinaison.QUINTE && mfamille[i] == 5)
                {
                    resultat = combinaison.QUINTE_FLUSH;
                }
                if (resultat == combinaison.PAIRE && resultat == combinaison.BRELAN)
                {
                    resultat = combinaison.FULL;
                }
                if (resultat != combinaison.QUINTE && mfamille[i] == 5)
                {
                    resultat = combinaison.COULEUR;
                }
                int c=0;
                for (int l = 0; l < similaire.Length; l++)// boucle qui parcour la liste de notre mains 
                {
                    if (similaire[l]==1)//si l'indice de la valeur du tableau est = 1 alors on rajoute 1 au compteur 
                    {
                        c += 1;
                    }
                    if (c==5) //si le compteur est = a 5 alors on verifie si la main crrespond à une possibilité de quinte 
                    {
                        for (int m = 0; m < 4; m++)// 1er boucle qui regarde si la 1er carte est = a la 1er de la liste  
                        {
                            int memcarte = 0;
                            for(int n = 0; n < 5; n++)//2ème boucle qui vérifie les cartes suivantes de la liste pour vérifier la possibilité de quinte
                            {
                                if (unJeu[n].valeur == quintes[m,n])// si le carte est = a selle dans la liste quinte 
                                {
                                    memcarte = memcarte + 1;// on rajoute 1 au "conteur" 
                                }
                                if (memcarte == 5)//si le conteur est = 5 cela veut dire que c'est une quinte
                                {
                                    resultat = combinaison.QUINTE;
                                }
                            }
                        }
                    }
                }
            }
            return resultat;
        }

        // Echange des cartes
        // Paramètres : le tableau de 5 cartes et le tableau des numéros des cartes à échanger
        private static void echangeCarte(ref carte[] unJeu, ref int[] e)
        {
            for (int i = 0; i < e.Length; i++) // e est un tableau qui contien les position des cartes que l'on veut échanger et les remplace par de nouvelles cartes grâce à la fonction tirage 
            {
                unJeu[e[i]] = tirage();
            }
        }

        // Tirage d'un jeu de 5 cartes
        // Paramètre : le tableau de 5 cartes à remplir
        private static void tirageDuJeu(ref carte[] unJeu)//fonctionne pas
        {
            for (int i = 0; i < 5; i++) //une boucle qui tire les 5 cartes du jeux 
            {
                do
                {
                    unJeu[i] = tirage();
                } 
                while (!carteUnique(unJeu[i], unJeu, i));


            }
        }
        // Affiche à l'écran une carte {valeur;famille} en fournisant la colonne de départ
        private static void affichageCarte(ref carte uneCarte)
        {
            //----------------------------
            // TIRAGE D'UN JEU DE 5 CARTES
            //----------------------------
            int left = 0;
            int c = 1;
            // Tirage aléatoire de 5 cartes
            for (int i = 0; i < 5; i++)
            {
                // Tirage de la carte n°i (le jeu doit être sans doublons !)

                // Affichage de la carte
                if (MonJeu[i].famille == '\u2665' || MonJeu[i].famille == '\u2666')
                    SetConsoleTextAttribute(hConsole, 252);
                else
                    SetConsoleTextAttribute(hConsole, 240);
                Console.SetCursorPosition(left, 5);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 6);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 7);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 8);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 9);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 11);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, (char)MonJeu[i].valeur, ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 12);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', ' ', ' ', ' ', ' ', ' ', ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 13);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.SetCursorPosition(left, 14);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, ' ', (char)MonJeu[i].famille, '|');
                Console.SetCursorPosition(left, 15);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.SetCursorPosition(left, 16);
                SetConsoleTextAttribute(hConsole, 10);
                Console.Write("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", ' ', ' ', ' ', ' ', ' ', c, ' ', ' ', ' ', ' ', ' ');
                left = left + 15;
                c++;
            }

        }

        //--------------------
        // Fonction PRINCIPALE
        //--------------------
        static void Main(string[] args)
        {
            //---------------
            // BOUCLE DU JEU
            //---------------
            string reponse;

            Console.OutputEncoding = Encoding.GetEncoding(65001);

            SetConsoleTextAttribute(hConsole, 012);
            while (true)
            {
                // Positionnement et affichage
                Console.Clear();
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', 'P', 'O', 'K', 'E', 'R', ' ', ' ', '|');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '|');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '1', ' ', 'J', 'o', 'u', 'e', 'r', ' ', '|');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '2', ' ', 'S', 'c', 'o', 'r', 'e', ' ', '|');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '|', ' ', '3', ' ', 'F', 'i', 'n', ' ', ' ', ' ', '|');
                Console.WriteLine("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}\n", '*', '-', '-', '-', '-', '-', '-', '-', '-', '-', '*');
                Console.WriteLine();
                // Lecture du choix


                do
                {
                    SetConsoleTextAttribute(hConsole, 014);
                    Console.Write("Votre choix : ");
                    reponse = Console.ReadLine();
                }
                while (reponse != "1" && reponse != "2" && reponse != "3");
                Console.Clear();
                SetConsoleTextAttribute(hConsole, 015);
                // Jouer au Poker
                if (reponse == "1")
                {
                    int i = 0;
                    tirageDuJeu(ref MonJeu);
                    affichageCarte(ref MonJeu[i]);

                    // Nombre de carte à échanger
                    try
                    {
                        int compteur = 0;
                        SetConsoleTextAttribute(hConsole, 012);
                        Console.Write("Nombre de cartes a echanger <0-5> ? : ");
                        compteur = int.Parse(Console.ReadLine());
                        int[] e = new int[compteur];
                        for (int j = 0; j < e.Length; j++)
                        {
                            Console.Write("Carte <1-5> : ");

                            e[j] = int.Parse(Console.ReadLine());
                            e[j] -= 1;
                        }

                        echangeCarte(ref MonJeu, ref e);

                    }
                    catch { }
                    //---------------------------------------
                    // CALCUL ET AFFICHAGE DU RESULTAT DU JEU
                    //---------------------------------------
                    Console.Clear();
                    affichageCarte(ref MonJeu[i]);
                    SetConsoleTextAttribute(hConsole, 012);
                    Console.Write("RESULTAT - Vous avez : ");
                    try
                    {
                        // Test de la combinaison
                        switch (cherche_combinaison(ref MonJeu))
                        {
                            case combinaison.RIEN:
                                Console.WriteLine("rien du tout... desole!"); break;
                            case combinaison.PAIRE:
                                Console.WriteLine("une simple paire..."); break;
                            case combinaison.DOUBLE_PAIRE:
                                Console.WriteLine("une double paire; on peut esperer..."); break;
                            case combinaison.BRELAN:
                                Console.WriteLine("un brelan; pas mal..."); break;
                            case combinaison.QUINTE:
                                Console.WriteLine("une quinte; bien!"); break;
                            case combinaison.FULL:
                                Console.WriteLine("un full; ouahh!"); break;
                            case combinaison.COULEUR:
                                Console.WriteLine("une couleur; bravo!"); break;
                            case combinaison.CARRE:
                                Console.WriteLine("un carre; champion!"); break;
                            case combinaison.QUINTE_FLUSH:
                                Console.WriteLine("une quinte-flush; royal!"); break;
                        };
                    }
                    catch { }
                    Console.ReadKey();
                    char enregister = ' ';
                    string nom = "";
                    BinaryWriter f;
                    SetConsoleTextAttribute(hConsole, 014);
                    Console.Write("Enregistrer le Jeu ? (O/N) : ");
                    enregister = char.Parse(Console.ReadLine());
                    enregister = Char.ToUpper(enregister);

                    if (enregister == 'O')
                    {
                        const string fileName = "scores.txt";
                        Console.WriteLine("Vous pouvez saisir votre nom (ou pseudo) : ");
                        nom = Console.ReadLine();
                        using (f = new BinaryWriter(new FileStream("scores.txt", FileMode.Append, FileAccess.Write)))
                        {
                            //Console.WriteLine();
                        }

                    }

                }
               if(reponse == "2")
                {
                    string articles;
                    char[] délimiteurs = { ';' };
                    carte UneCarte;
                    string nom;
                    if (File.Exists("scores.txt"))
                    {
                        using (BinaryReader f = new BinaryReader(new FileStream("scores.txt", FileMode.Open, FileAccess.Read)))
                        {
      

                        }

                        //Console.WriteLine("Nom : " + nom);
                        Console.ReadKey();
                    }
                }

                if (reponse == "3") // réponse 3 = fin donc le jeux se ferme 
                    break;

            }
            Console.Clear();
            Console.ReadKey();
        }
    }
}