using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// Represents a container of components
    /// </summary>
    public class Actor
    {
        /// <summary>
        /// The parent of this actor
        /// </summary>
        private Actor parent;
        public Actor Parent
        {
            get { return parent; }
            set
            {
                if (parent != null) parent.children.Remove(this);
                parent = value;
                if (parent != null) parent.children.Add(this);
            }
        }

        /// <summary>
        /// The children of this actor
        /// </summary>
        private HashSet<Actor> children;
        public IEnumerable<Actor> Children
        {
            get { return children; }
        }

        /// <summary>
        /// The root parent of this actor
        /// </summary>
        public Actor Root
        {
            get
            {
                if (Parent == null) return this;
                return Parent.Root;
            }
        }

        /// <summary>
        /// Compares two given components based on their priority
        /// </summary>
        private class ComponentPriorityComparer : IComparer<BaseComponent>
        {
            public int Compare(BaseComponent x, BaseComponent y)
            {
                return Comparer<int>.Default.Compare(x.Priority, y.Priority);
            }
        }

        private static readonly ComponentPriorityComparer comparer = new ComponentPriorityComparer();

        /// <summary>
        /// A string identifying this actor
        /// </summary>
        public string Name { get; set; }

        private OrderedList<BaseComponent> components; // All components "owned" by this Actor
        private OrderedList<BaseComponent> components_uninit; // All components "owned" but not yet initialised by this Actor

        /// <summary>
        /// Gets if this actor has been destroyed or not
        /// </summary>
        public bool Destroyed { get; private set; }

        /// <summary>
        /// Gets or sets the MessagePool associated with this Actor
        /// </summary>
        public MessagePool MessagePool { get; set; }

        public Actor(MessagePool pool)
        {
            // Initialise component list and message pool
            components = new OrderedList<BaseComponent>(comparer);
            components_uninit = new OrderedList<BaseComponent>(comparer);
            MessagePool = pool;
            children = new HashSet<Actor>();

            // We're fresh
            Destroyed = false;
            Name = "Actor";
        }

        /// <summary>
        /// Adds and returns a new instance of a component to this Actor
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public BaseComponent AddComponent(Type type)
        {
            // Check for required components
            RequiresComponent requires = type.GetCustomAttribute<RequiresComponent>();
            if (requires != null)
            {
                bool found = components.Any((x) => x.GetType() == requires.RequiredType) || components_uninit.Any((x) => x.GetType() == requires.RequiredType);
                if (!found)
                {
                    Console.WriteLine("Component {0} requires component {1} but it was not found!", type.Name, requires.RequiredType.Name);
                    return null;
                }
            }

            // Create the component
            var c = Activator.CreateInstance(type) as BaseComponent;

            // Attach it to us
            if (!components_uninit.Add(c))
            {
                Console.WriteLine("Failed to add component {0} to actor (priority was the same as an existing component, or the GetHashCode hack failed)", type.Name);
                return null;
            }
            c.Owner = this;

            // Return it
            return c;
        }

        /// <summary>
        /// Adds and returns a new instance of a component to this Actor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : BaseComponent
        {
            // Check for required components
            RequiresComponent requires = typeof(T).GetCustomAttribute<RequiresComponent>();
            if (requires != null)
            {
                bool found = components.Any((x) => x.GetType() == requires.RequiredType) || components_uninit.Any((x) => x.GetType() == requires.RequiredType);
                if (!found)
                {
                    Console.WriteLine("Component {0} requires component {1} but it was not found!", typeof(T).Name, requires.RequiredType.Name);
                    return null;
                }
            }

            // Create the component
            var c = Activator.CreateInstance<T>();

            // Attach it to us
            if (!components_uninit.Add(c))
            {
                Console.WriteLine("Failed to add component {0} to actor (priority was the same as an existing component, or the GetHashCode hack failed)", typeof(T).Name);
                return null;
            }
            c.Owner = this;

            // Return it
            return c as T;
        }

        /// <summary>
        /// Gets an existing component attached to this Actor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public BaseComponent GetComponent(Type type, bool allowunit = false)
        {
            // Loop until we find it
            foreach (var c in components)
            {
                var ct = c.GetType();
                if (type.IsAssignableFrom(ct))
                    return c;
            }
            if (allowunit)
            {
                foreach (var c in components_uninit)
                {
                    var ct = c.GetType();
                    if (type.IsAssignableFrom(ct))
                        return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an existing component attached to this Actor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : BaseComponent
        {
            // Loop until we find it
            var t = typeof(T);
            foreach (var c in components)
            {
                var ct = c.GetType();
                if (t.IsAssignableFrom(ct))
                    return c as T;
            }
            return null;
        }

        /// <summary>
        /// Removes a component from this Actor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : BaseComponent
        {
            // Loop until we find it
            var t = typeof(T);
            foreach (var c in components)
                if (c.GetType() == t)
                {
                    // Remove and done
                    c.OnDetach();
                    components.Remove(c);
                    return;
                }
        }

        /// <summary>
        /// Initialises all components attached to this Actor
        /// </summary>
        public void Init()
        {
            // Attach all components
            foreach (var c in components_uninit)
            {
                if (!components.Add(c))
                {
                    Console.WriteLine("Failed to initialise component {0} on actor (priority was the same as an existing component, or the GetHashCode hack failed)", c.GetType().Name);
                }
                else
                    c.OnAttach();
            }

            // Clear uninit
            components_uninit.Clear();
        }

        /// <summary>
        /// Destroys this Actor and cleans up all attached components
        /// </summary>
        public void Destroy(bool destroychildren = false)
        {
            // Sanity check
            if (Destroyed) return;

            // Destroy and unlink all components
            Destroyed = true;
            foreach (var c in components)
            {
                c.OnDetach();
                c.Owner = null;
            }
            components = null;

            // Destroy any uninit components
            foreach (var c in components_uninit)
                c.Owner = null;
            components_uninit = null;

            // Remove parent
            Parent = null;

            // Recursively destroy all children
            if (destroychildren)
                foreach (var c in children.ToArray())
                    c.Destroy(true);
        }

        /// <summary>
        /// Sends a message to this actor and optionally all children
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="includechildren"></param>
        public void SendLocalMessage(Message msg, bool includechildren = false)
        {
            // Handle the message ourselves
            foreach (var c in components.ToArray())
                c.HandleMessage(msg);

            // Pass it on
            if (includechildren)
                foreach (var c in children)
                    c.SendLocalMessage(msg, true);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
