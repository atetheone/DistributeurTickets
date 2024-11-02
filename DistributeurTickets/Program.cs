using System;
using System.IO;

namespace DistributeurTickets
{
    class Program
    {
        static List<Client> listeClients = new List<Client>();
        private static string fichierClients = Path.Combine(Path.GetTempPath(), "clients.txt");
        
        static void Main(string[] args)
        {
            string fichier = Path.Combine(Path.GetTempPath(), "fnumero.txt");
            bool continuer = true;

            int compteurVersement = 1, compteurRetrait = 1, compteurInformations = 1;

            // Charger les numéros précédents si le fichier existe
            ChargerDonnees(fichier, ref compteurVersement, ref compteurRetrait, ref compteurInformations);

            // Charger les informations des clients de sessions antérieures
            ChargerClientsAnterieurs();

            // Niveau 1 : Choix de l'Opération
            while (continuer)
            {
                Console.WriteLine("Quel type d'opération souhaitez-vous effectuer ?");
                Console.WriteLine("1 - Versement");
                Console.WriteLine("2 - Retrait");
                Console.WriteLine("3 - Informations");
                Console.WriteLine("10 - Quitter");
                Console.Write("Votre choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        GenererTicketsPourType("V", ref compteurVersement, fichier, "Versement", ref compteurVersement, ref compteurRetrait, ref compteurInformations);
                        break;

                    case "2":
                        GenererTicketsPourType("R", ref compteurRetrait, fichier, "Retrait", ref compteurVersement, ref compteurRetrait, ref compteurInformations);
                        break;

                    case "3":
                        GenererTicketsPourType("I", ref compteurInformations, fichier, "Informations", ref compteurVersement, ref compteurRetrait, ref compteurInformations);
                        break;

                    case "10":
                        continuer = false;
                        AfficherListeClients();
                        break;

                    default:
                        Console.WriteLine("Veuillez saisir un choix valide svp (1, 2, 3 ou 10).");
                        break;
                }
            }
            Console.WriteLine("\nFin du programme...");
            
        }
        
        // Fonction pour générer plusieurs tickets pour le même type d'opération
        static void GenererTicketsPourType(
            string type, 
            ref int compteur, 
            string fichier, 
            string operation, 
            ref int compteurVersement, 
            ref int compteurRetrait, 
            ref int compteurInformations)
        {
            bool continuerAvecMemeOperation = true;
            while (continuerAvecMemeOperation)
            {
                // Collecte des informations du client
                
                string numeroCompte;
                do
                {
                    Console.Write("Entrez le numéro de compte du client : ");
                    numeroCompte = Console.ReadLine();
    
                    if (string.IsNullOrWhiteSpace(numeroCompte))
                    {
                        Console.WriteLine("Le numéro de compte ne peut pas être vide. Veuillez entrer un numéro valide.");
                    }
                    else if (!VerifierUniciteNumeroCompte(numeroCompte))
                    {
                        Console.WriteLine("Numéro de compte déjà utilisé. Veuillez entrer un nouveau numéro.");
                    }

                } while (string.IsNullOrWhiteSpace(numeroCompte) || !VerifierUniciteNumeroCompte(numeroCompte));

                
                string nom = SaisirChaineNonVide("Entrez le nom du client : ");
                
                string prenom = SaisirChaineNonVide("Entrez le prénom du client : ");

                int enAttente = compteur - 1;
                string numeroTicket = $"{type}-{compteur}";
                compteur++;

                // Ajout du client à la liste des clients
                Client client = new Client(numeroCompte, nom, prenom, numeroTicket);
                listeClients.Add(client);

                // Sauvegarde des informations du client dans le fichier des clients
                SauvegarderClient(client);

                // Affichage des informations sur le ticket
                string message = enAttente == 0 ? "Vous êtes le premier" : $"Il y a {enAttente} personne(s) qui attendent avant vous.";
                Console.WriteLine($"\nVotre numéro de ticket : {numeroTicket}, {message}");

                // Sauvegarde des compteurs dans le fichier
                SauvegarderDonnees(fichier, compteurVersement, compteurRetrait, compteurInformations);

                // Demande à l'utilisateur s'il souhaite générer un autre ticket pour le même type
                Console.Write($"Souhaitez-vous un autre ticket pour {operation} ? (o/n) : ");
                string reponse = Console.ReadLine().ToLower();

                if (reponse == "n")
                {
                    continuerAvecMemeOperation = false; // Sortie de la boucle, retour au menu principal
                }
                else if (reponse != "o")
                {
                    Console.WriteLine("Réponse invalide. Veuillez répondre par 'o' ou 'n'.");
                }
            }
            Console.Clear();
        }
        
        // Fonction pour obtenir une chaîne non vide en affichant un message spécifique
        private static string SaisirChaineNonVide(string message)
        {
            string saisie;
            do
            {
                Console.Write(message);
                saisie = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(saisie))
                {
                    Console.WriteLine("La saisie ne peut pas être vide. Veuillez entrer une valeur valide.");
                }
            } while (string.IsNullOrWhiteSpace(saisie));

            return saisie;
        }

        
        // Fonction pour charger les clients de sessions antérieures
        static void ChargerClientsAnterieurs()
        {
            if (File.Exists(fichierClients))
            {
                string[] lignes = File.ReadAllLines(fichierClients);
                foreach (string ligne in lignes)
                {
                    string[] parts = ligne.Split(';');
                    if (parts.Length == 4)
                    {
                        string numeroCompte = parts[0];
                        string nom = parts[1];
                        string prenom = parts[2];
                        string numeroTicket = parts[3];
                        listeClients.Add(new Client(numeroCompte, nom, prenom, numeroTicket));
                    }
                }
            }
        }
        
        // Fonction pour vérifier l'unicité du numéro de compte
        static bool VerifierUniciteNumeroCompte(string numeroCompte)
        {
            foreach (var client in listeClients)
            {
                if (client.NumeroCompte == numeroCompte)
                    return false;
            }
            return true;
        }
        
        // Fonction pour afficher la liste finale des clients
        static void AfficherListeClients()
        {
            Console.WriteLine("\nListe des clients servis :");
            foreach (var client in listeClients)
            {
                Console.WriteLine($"Compte : {client.NumeroCompte}, Nom : {client.Nom}, Prénom : {client.Prenom}, Ticket : {client.NumeroTicket}");
            }
        }
        
        // Fonction pour sauvegarder un client dans le fichier des clients
        static void SauvegarderClient(Client client)
        {
            try
            {
                string ligne = $"{client.NumeroCompte};{client.Nom};{client.Prenom};{client.NumeroTicket}";
                File.AppendAllText(fichierClients, ligne + Environment.NewLine);
            }
            catch (IOException e)
            {
                Console.WriteLine("Erreur lors de la sauvegarde des informations du client : " + e.Message);
            }
            
        }

        // Fonction pour charger les données depuis le fichier
        static void ChargerDonnees(string fichier, ref int compteurVersement, ref int compteurRetrait, ref int compteurInformations)
        {
            if (File.Exists(fichier))
            {
                string[] lignes = File.ReadAllLines(fichier);
                if (lignes.Length >= 3)
                {
                    int.TryParse(lignes[0], out compteurVersement);
                    int.TryParse(lignes[1], out compteurRetrait);
                    int.TryParse(lignes[2], out compteurInformations);
                }
            }
        }

        // Fonction pour attribuer un numéro de ticket
        static string AttribuerNumeroTicket(string type, ref int compteur, out int enAttente)
        {
            enAttente = compteur - 1;
            string numeroTicket = $"{type}-{compteur}";
            compteur++;
            return numeroTicket;
        }

        // Fonction pour sauvegarder les données dans le fichier
        static void SauvegarderDonnees(string fichier, int compteurVersement, int compteurRetrait, int compteurInformations)
        {
            try
            {
                File.WriteAllLines(
                    fichier,
                    new string[]
                    {
                        compteurVersement.ToString(),
                        compteurRetrait.ToString(),
                        compteurInformations.ToString()
                    }
                );
            }
            catch (IOException e)
            {
                Console.WriteLine("Erreur lors de la sauvegarde des données : " + e.Message);
            }
        }
    }

    class Client
    {
        public string NumeroCompte { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string NumeroTicket { get; set; }

        public Client(string numeroCompte, string nom, string prenom, string numeroTicket)
        {
            NumeroCompte = numeroCompte;
            Nom = nom;
            Prenom = prenom;
            NumeroTicket = numeroTicket;
        }
    }
}
