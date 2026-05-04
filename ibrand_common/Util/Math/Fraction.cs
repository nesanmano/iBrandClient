using System;
using JetBrains.Annotations;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.Common.Util.Math
{
    /// Cette classe représente un nombre fractionnaire exprimé avec un numérateur et un dénominateur entier.
    /// Plusieures représentation mathématique aime ce genre de représentation pour des nombres qui seraient complexes
    /// à exprimer si on utilisait un double, ou qu'il y aurait des perte dû au système de stockage de chiffre flottant.
    /// En clair, on ne stocke pas de mantisse, donc pas de perte. Bien sûr, pour des calculs internes avec valeurs, il faut faire
    /// la conversion, mais certains systèmes peuvent agir directement sur le calcul des numérateurs/dénominateur.
    /// Par example, (2/3) * 2 est égal à (4/3). Ce qui est plus rapide au niveau système que:
    /// 0.666666666666666 * 2.0 = 1.3333333333333
    /// Sans prendre en compte le fait que il y aura une imprécision dû à l'arrondissement.
    /// Il est également possible de représenter des nombres impossibles autrement, comme "Infinity/0"
    [PublicAPI]
    public class Fraction : IEquatable<Fraction>
    {
        private int _num;
        private int _dem;
        
        public int Numerator
        {
            get => _num;
            set
            {
                _num = value;
                Update();
            }
        }

        public int Denominator
        {
            get => _dem;
            set
            {
                _dem = value;
                Update();
            }
        }
        
        public double DoubleValue { get; private set; }

        /// Constructeur vide.
        public Fraction(){}
        
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="numerator">Valeur du numérateur.</param>
        /// <param name="denominator">Valeur du dénominateur.</param>
        public Fraction(int numerator, int denominator)
        {
            _num = numerator;
            _dem = denominator;
            Update();
        }

        private void Update()
        {
            if (_dem == 0)
            {
                DoubleValue = double.NaN;
                return;
            } 
                
            DoubleValue = _num / (double)_dem;
        }
        
        public override string ToString()
        {
            return $"{_num}/{_dem}";
        }

        public bool Equals(Fraction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _num == other._num && _dem == other._dem;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Fraction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _num;
                hashCode = (hashCode * 397) ^ _dem;
                return hashCode;
            }
        }

        public static bool operator ==(Fraction left, Fraction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Fraction left, Fraction right)
        {
            return !Equals(left, right);
        }
    }
}