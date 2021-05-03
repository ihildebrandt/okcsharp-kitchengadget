import { useState } from 'react';
import './App.css';

function swapRecipe(id) {
  var did = `recipe_${id}`;
  var recipeBlocks = document.getElementsByClassName("recipe");
  for (var i = 0; i < recipeBlocks.length; i++) {
    var r = recipeBlocks[i];
    if (r.id === did) continue;
    if (r.classList.contains("hidden")) continue;
    r.classList.add("hidden");
  }

  var selectedBlock = document.getElementById(did);
  console.log(selectedBlock);
  if (selectedBlock.classList.contains("hidden")) {
    selectedBlock.classList.remove("hidden");
  } else {
    selectedBlock.classList.add("hidden");
  }
}

function App() {
  const [content, setContent] = useState({ loading: false });
  
  if (!content.loading && !content.current) {
    setContent({ loading: true });

    fetch("https://localhost:5001/recipes")
      .then((response) => {
        response.json()
          .then((json) => {
            json.current = true;
            setContent(json);
          });
    });
  }
  
  return (
    <div className="App">
      <header className="App-header">
        <h1>Recipe Box</h1>
      </header>
      <section>
        <h2>Recipe List</h2>
        {content.current && 
        <ul>
          {content.items.map(recipe => (
            <li key={recipe.id}>
              <a href={(`#recipe_${recipe.id}`)} onClick={() => swapRecipe(recipe.id)}>{recipe.name}</a>
              <div className="hidden recipe" id={(`recipe_${recipe.id}`)}>
                <h3>Ingredients</h3>
                <ul>
                  {recipe.ingredients.map(ingredient => (
                    <li key={ingredient.id}>{ingredient.amount} {ingredient.measure} {ingredient.name}</li>
                  ))}
                </ul>
                <h3>Instructions</h3>
                <ol>
                  {recipe.instructions.map(instruction => (
                    <li key={instruction.id}>{instruction.text}</li>
                  ))}
                </ol>
              </div>
            </li>
          ))}
        </ul>
        }
      </section>
    </div>
  );
}

export default App;
