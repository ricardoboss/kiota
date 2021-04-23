﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Kiota.Builder
{
    public enum CodeClassKind {
        Custom,
        RequestBuilder,
        Model,
        QueryParameters,
    }
    /// <summary>
    /// CodeClass represents an instance of a Class to be generated in source code
    /// </summary>
    public class CodeClass : CodeBlock, IDocumentedElement
    {
        private string name;

        public CodeClass(CodeElement parent):base(parent)
        {
            StartBlock = new Declaration(this);
            EndBlock = new End(this);
        }
        public CodeClassKind ClassKind { get; set; } = CodeClassKind.Custom;

        public string Description {get; set;}
        /// <summary>
        /// Name of Class
        /// </summary>
        public override string Name
        {
            get => name;
            set
            {
                name = value;
                StartBlock = new Declaration(this) { Name = name };
            }
        }

        public void SetIndexer(CodeIndexer indexer)
        {
            AddRange(indexer);
        }

        public void AddProperty(params CodeProperty[] properties)
        {
            if(!properties.Any() || properties.Any(x => x == null))
                throw new ArgumentNullException(nameof(properties));
            AddMissingParent(properties);
            AddRange(properties);
        }

        public bool ContainsMember(string name)
        {
            return this.InnerChildElements.ContainsKey(name);
        }

        public void AddMethod(params CodeMethod[] methods)
        {
            if(!methods.Any() || methods.Any(x => x == null))
                throw new ArgumentOutOfRangeException(nameof(methods));
            AddMissingParent(methods);
            AddRange(methods);
        }

        public void AddInnerClass(params CodeClass[] codeClasses)
        {
            if(!codeClasses.Any() || codeClasses.Any(x => x == null))
                throw new ArgumentOutOfRangeException(nameof(codeClasses));
            AddMissingParent(codeClasses);
            AddRange(codeClasses);
        }
        public CodeClass GetParentClass() {
            if(StartBlock is Declaration declaration)
                return declaration.Inherits?.TypeDefinition as CodeClass;
            else return null;
        }
        
        public CodeClass GetGreatestGrandparent(CodeClass startClassToSkip = null) {
            var parentClass = GetParentClass();
            if(parentClass == null)
                return startClassToSkip != null && startClassToSkip == this ? null : this;
            // we don't want to return the current class if this is the start node in the inheritance tree and doesn't have parent
            else
                return parentClass.GetGreatestGrandparent(startClassToSkip);
        }

        public class Declaration : BlockDeclaration
        {
            public Declaration(CodeElement parent):base(parent)
            {
                
            }
            public CodeType Inherits { get; set; }
            public List<CodeType> Implements { get; set; } = new List<CodeType>();
        }

        public class End : BlockEnd
        {
            public End(CodeElement parent):base(parent)
            {
                
            }
        }
    }
}
