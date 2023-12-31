﻿namespace DiagnosticsAgent.Counters.Producer;

internal static class CounterCollectionParser
{
    internal static Dictionary<string, string[]?> Parse(ReadOnlySpan<char> collectionString)
    {
        var result = new Dictionary<string, string[]?>();

        if (collectionString.IsEmpty)
        {
            return result;
        }

        var insideInternalList = false;
        var itemStartIndex = 0;
        (string Key, string[]? Collection)? item;

        for (var i = 0; i < collectionString.Length; i++)
        {
            if (collectionString[i] == '[')
            {
                insideInternalList = true;
                continue;
            }

            if (collectionString[i] == ']')
            {
                insideInternalList = false;
                continue;
            }

            if (collectionString[i] != ',' || insideInternalList)
            {
                continue;
            }

            item = ParseItem(collectionString.Slice(itemStartIndex, i - itemStartIndex).Trim());
            if (item.HasValue)
            {
                result.Add(item.Value.Key, item.Value.Collection);
            }

            itemStartIndex = i + 1;
        }

        item = ParseItem(collectionString.Slice(itemStartIndex).Trim());
        if (item.HasValue)
        {
            result.Add(item.Value.Key, item.Value.Collection);
        }

        return result;
    }
    
    private static (string Key, string[]? Collection)? ParseItem(ReadOnlySpan<char> itemString)
    {
        if (itemString.IsEmpty)
        {
            return null;
        }

        var itemCollectionIndex = itemString.IndexOf('[');
        if (itemCollectionIndex == -1)
        {
            return (itemString.ToString(), null);
        }

        var key = itemString.Slice(0, itemCollectionIndex).Trim();

        var itemCollectionLength = itemString.Length - itemCollectionIndex - 2;
        var itemCollection = itemString.Slice(itemCollectionIndex + 1, itemCollectionLength).Trim();
        if (itemCollection.IsEmpty)
        {
            return (key.ToString(), null);
        }

        var counters = ParseItemCollection(itemCollection);

        return (key.ToString(), counters);
    }

    private static string[]? ParseItemCollection(ReadOnlySpan<char> itemCollectionString)
    {
        var collection = new List<string>();

        ReadOnlySpan<char> internalItem;
        var itemCollection = itemCollectionString;
        var delimiterIndex = itemCollection.IndexOf(',');

        while (delimiterIndex != -1)
        {
            internalItem = itemCollection.Slice(0, delimiterIndex).Trim();
            if (!internalItem.IsEmpty)
            {
                collection.Add(internalItem.ToString());
            }

            itemCollection = itemCollection.Slice(delimiterIndex + 1);
            delimiterIndex = itemCollection.IndexOf(',');
        }

        internalItem = itemCollection.Trim();
        if (!internalItem.IsEmpty)
        {
            collection.Add(internalItem.ToString());
        }

        return collection.Count != 0 ? collection.ToArray() : null;
    }
}