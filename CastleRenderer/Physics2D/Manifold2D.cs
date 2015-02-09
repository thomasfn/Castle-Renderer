using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents data about a collision between two objects
    /// </summary>
    public struct Manifold2D
    {
        // The physics objects involved in the collision
        public IPhysicsObject2D A, B;

        // The penetration of A into B
        public float Penetration;

        // The direction of the collision of A into B
        public Vector2 Normal;

        // The contacts of the collision in world space
        public int NumContacts;
        public Vector2 Contact1, Contact2;

        /// <summary>
        /// Adds a contact to this manifold
        /// </summary>
        /// <param name="contact"></param>
        public void AddContact(Vector2 contact)
        {
            if (NumContacts == 0)
                Contact1 = contact;
            else if (NumContacts == 1)
                Contact2 = contact;
            else
                throw new InvalidOperationException("Maximum of 2 contacts per manifold");
            NumContacts++;
        }

        /// <summary>
        /// Gets a uniform contact point for this manifold
        /// </summary>
        /// <returns></returns>
        public Vector2 GetUniformContactPoint()
        {
            switch (NumContacts)
            {
                case 1:
                    return Contact1;
                case 2:
                    return Vector2.Lerp(Contact1, Contact2, 0.5f);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Applies an impulse to both bodies at the contact points
        /// </summary>
        /// <param name="impulse"></param>
        public void ApplyImpulseAtContacts(Vector2 impulse)
        {
            // Switch on contact count
            switch (NumContacts)
            {
                case 0:
                    A.ApplyImpulse(impulse);
                    B.ApplyImpulse(-impulse);
                    break;
                case 1:
                    A.ApplyImpulse(-impulse, Contact1 - A.Position);
                    B.ApplyImpulse(impulse, Contact1 - B.Position);
                    break;
                case 2:
                    A.ApplyImpulse(impulse * -0.5f, A.WorldToObject(Contact1));
                    A.ApplyImpulse(impulse * -0.5f, A.WorldToObject(Contact2));
                    B.ApplyImpulse(impulse * 0.5f, B.WorldToObject(Contact1));
                    B.ApplyImpulse(impulse * 0.5f, B.WorldToObject(Contact2));
                    break;
                default:
                    throw new InvalidOperationException("Maximum of 2 contacts per manifold");
            }
        }
    }
}
